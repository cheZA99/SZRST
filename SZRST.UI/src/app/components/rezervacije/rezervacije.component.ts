import { Component, OnInit } from '@angular/core';
import { CalendarOptions, EventInput } from '@fullcalendar/core';
import timeGridPlugin from '@fullcalendar/timegrid';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';
import { AppointmentService } from '../../services/appointment.service';
import { AppointmentCalendar } from 'src/app/types/appointment-calendar-model';
import { ViewChild } from '@angular/core';
import { FullCalendarComponent } from '@fullcalendar/angular';
import enGbLocale from '@fullcalendar/core/locales/en-gb';
import { MatDialog } from '@angular/material/dialog';
import { AppointmentDialogComponent } from '../rezervacije-dialog/rezervacije-dialog.component';
import { forkJoin } from 'rxjs';
import { FacilityService } from 'src/app/services/facility.service';
import { AppointmentTypeService } from 'src/app/services/appointment-type.service';
import { TenantService, Tenant } from 'src/app/services/tenant.service';
import { UserService, User } from 'src/app/services/user.service';
import { AuthService } from 'src/app/services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-rezervacije',
  templateUrl: './rezervacije.component.html',
  styleUrls: ['./rezervacije.component.css'],
})
export class RezervacijeComponent implements OnInit {
  @ViewChild(FullCalendarComponent)
  calendarComponent!: FullCalendarComponent;

  selectedFacilityId: number | null = null;
  selectedTenantId: number | null = null;
  tenantIdFromToken: number | null = null;
  facilities: any[] = [];
  filteredFacilities: any[] = [];
  tenants: Tenant[] = [];

  currentUserId: string | null = null;
  userRoles: string[] = [];

  isKorisnik = false;
  isSuperAdmin = false;
  isAdmin = false;
  isUposlenik = false;

  showTenantFilter = false;
  canSelectUserInModal = false;

  calendarOptions: CalendarOptions = {
    locale: enGbLocale,
    initialView: 'timeGridWeek',
    plugins: [timeGridPlugin, dayGridPlugin, interactionPlugin],
    selectable: true,
    editable: true,
    eventDurationEditable: false,
    eventResizableFromStart: false,
    allDaySlot: false,
    slotMinTime: '07:00:00',
    slotMaxTime: '20:00:00',
    slotDuration: '00:30:00',
    defaultTimedEventDuration: '01:00:00',

    slotLabelFormat: {
      hour: '2-digit',
      minute: '2-digit',
      hour12: false,
    },

    eventTimeFormat: {
      hour: '2-digit',
      minute: '2-digit',
      hour12: false,
    },

    events: (info, successCallback, failureCallback) => {
      this.loadAppointments(
        info.startStr,
        info.endStr,
        successCallback,
        failureCallback
      );
    },

    dateClick: (info) => {
      this.openCreateModal(info.date);
    },

    eventClick: (info) => {
      const appointmentUserId = info.event.extendedProps['userId'];

      if (!this.canEditAppointment(appointmentUserId)) {
        return;
      }

      this.openEditModal(info);
    },
    eventResize: (info) => {
      info.event.setDates(info.oldEvent.start!, info.oldEvent.end!);
      this.toastr.warning('Resizing appointments is not allowed', 'Warning');
    },
    eventDrop: (info) => {
      const appointmentUserId = info.event.extendedProps['userId'];

      if (!this.canEditAppointment(appointmentUserId)) {
        info.event.setDates(info.oldEvent.start!, info.oldEvent.end!);
        this.toastr.warning(
          'You can only edit your own appointments',
          'Warning'
        );
        return;
      }

      const oldStart = info.oldEvent.start!;
      const newStart = info.event.start!;
      const oldEnd = info.oldEvent.end!;
      const newEnd = info.event.end!;

      const oldDuration = oldEnd.getTime() - oldStart.getTime();
      const newDuration = newEnd.getTime() - newStart.getTime();

      if (oldDuration !== newDuration) {
        info.event.setDates(info.oldEvent.start!, info.oldEvent.end!);
        this.toastr.warning(
          'Cannot change appointment duration. Duration is fixed by appointment type.',
          'Warning'
        );
        return;
      }

      const calendarApi = this.calendarComponent.getApi();
      const allEvents = calendarApi.getEvents();

      for (let event of allEvents) {
        if (event.id === info.event.id) continue;

        const eventStart = event.start!;
        const eventEnd = event.end!;

        if (newStart < eventEnd && newEnd > eventStart) {
          info.event.setDates(info.oldEvent.start!, info.oldEvent.end!);
          this.toastr.warning(
            'Cannot overlap with existing appointments',
            'Warning'
          );
          return;
        }
      }

      this.updateAppointmentTime(+info.event.id!, info.event.startStr!);
    },
  };

  constructor(
    private appointmentService: AppointmentService,
    private facilityService: FacilityService,
    private appointmentTypeService: AppointmentTypeService,
    private tenantService: TenantService,
    private userService: UserService,
    private authService: AuthService,
    private dialog: MatDialog,
    private toastr: ToastrService
  ) {}

  ngOnInit() {
    this.initializeUserData();

    if (this.showTenantFilter) {
      this.loadTenants();
    }

    this.loadFacilities();
  }

  initializeUserData() {
    const decoded = this.authService.getDecodedToken();
    if (decoded) {
      this.currentUserId = decoded.sub || decoded.userId || decoded.nameid;
      this.userRoles = this.authService.getUserRoles();

      this.isKorisnik = this.authService.hasRole('Korisnik');
      this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
      this.isAdmin = this.authService.hasRole('Admin');
      this.isUposlenik = this.authService.hasRole('Uposlenik');

      this.showTenantFilter = this.isKorisnik || this.isSuperAdmin;

      this.canSelectUserInModal = this.isAdmin || this.isUposlenik;

      if ((this.isAdmin || this.isUposlenik) && decoded.tenantId) {
        this.tenantIdFromToken = +decoded.tenantId;
        this.selectedTenantId = this.tenantIdFromToken;
      }
    }
  }

  loadFacilities() {
    this.facilityService.getAll().subscribe({
      next: (data) => {
        this.facilities = data;
        this.applyFacilityFilter();
      },
      error: (err) => {
        this.toastr.error('Failed to load facilities', 'Error');
      },
    });
  }

  loadTenants() {
    this.tenantService.getAllTenants().subscribe({
      next: (data) => {
        this.tenants = data;
      },
      error: (err) => {
        this.toastr.error('Failed to load organizations', 'Error');
      },
    });
  }
  applyFacilityFilter() {
    if (this.selectedTenantId) {
      this.filteredFacilities = this.facilities.filter(
        (f) => f.tenantId === this.selectedTenantId
      );
    } else {
      this.filteredFacilities = [];
    }

    if (
      this.selectedFacilityId &&
      !this.filteredFacilities.some((f) => f.id === this.selectedFacilityId)
    ) {
      this.selectedFacilityId = null;
    }
  }

  loadAppointments(
    startStr: string,
    endStr: string,
    successCallback: any,
    failureCallback: any
  ) {
    const params: any = {
      from: startStr,
      to: endStr,
    };

    if (this.selectedFacilityId) {
      params.facilityId = this.selectedFacilityId;
    }

    if (this.selectedTenantId) {
      params.tenantId = this.selectedTenantId;
    }

    if (this.isKorisnik && this.currentUserId) {
      params.userId = this.currentUserId;
    }

    this.appointmentService
      .getCalendar(startStr, endStr, params.facilityId, params.tenantId)
      .subscribe({
        next: (data) => {
          const events = data.map((a) => this.mapEvent(a));
          successCallback(events);
        },
        error: (err) => {
          this.toastr.error('Failed to load appointments', 'Error');
          failureCallback(err);
        },
      });
  }

  onFacilityFilterChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const value = select.value;
    this.selectedFacilityId = value ? parseInt(value, 10) : null;
    this.refreshCalendar();
  }

  get canShowCalendar(): boolean {
    if (!this.showTenantFilter) {
      return !!this.selectedFacilityId;
    }

    return !!this.selectedTenantId && !!this.selectedFacilityId;
  }

  onTenantFilterChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const value = select.value;
    this.selectedTenantId = value ? parseInt(value, 10) : null;

    this.applyFacilityFilter();
    this.refreshCalendar();
  }

  canEditAppointment(
    appointmentUserId: string | number | null | undefined
  ): boolean {
    if (this.isKorisnik) {
      if (!appointmentUserId || !this.currentUserId) return false;

      return appointmentUserId.toString() === this.currentUserId.toString();
    }

    return true;
  }

  mapEvent(a: AppointmentCalendar): EventInput {
    const canInteract = this.canEditAppointment(a.userId);

    return {
      id: a.id.toString(),
      title: `${a.appointmentTypeName} • ${a.facilityName}`,
      start: a.start,
      end: a.end,
      editable: canInteract,
      className: canInteract ? '' : 'not-editable',
      extendedProps: {
        userId: a.userId?.toString() ?? null,
        facilityId: a.facilityId,
        appointmentTypeId: a.appointmentTypeId,
        tenantId: a.tenantId,
        isFree: a.isFree,
        isClosed: a.isClosed,
      },
    };
  }

  openCreateModal(date?: Date) {
    const currentFacilityId = this.selectedFacilityId;

    const requests: any = {
      facilities: this.facilityService.getAll(),
      appointmentTypes: this.appointmentTypeService.getAll(),
    };

    if (this.canSelectUserInModal) {
      requests.users = this.userService.getUsersForAppointments();
    }

    forkJoin(requests).subscribe({
      next: (data: any) => {
        let filteredFacilities = data.facilities;
        let filteredTypes = data.appointmentTypes;

        if (this.selectedTenantId) {
          filteredFacilities = filteredFacilities.filter(
            (f: any) => f.tenantId === this.selectedTenantId
          );
          filteredTypes = filteredTypes.filter(
            (t: any) => t.tenantId === this.selectedTenantId
          );
        }

        let selectedTime = '';
        if (date) {
          const hours = date.getHours().toString().padStart(2, '0');
          const minutes = date.getMinutes().toString().padStart(2, '0');
          selectedTime = `${hours}:${minutes}`;
        }

        const dialogRef = this.dialog.open(AppointmentDialogComponent, {
          width: '520px',
          maxWidth: '95vw',
          autoFocus: false,
          disableClose: true,
          hasBackdrop: true,
          restoreFocus: false,
          panelClass: 'appointment-dialog-center',
          data: {
            facilities: filteredFacilities,
            appointmentTypes: filteredTypes,
            users: data.users || [],
            initialDate: date, 
            canSelectUser: this.canSelectUserInModal,
            currentUserId: this.currentUserId,
            tenantId: this.selectedTenantId,
            preSelectedFacilityId: currentFacilityId,
          },
        });

        dialogRef.afterClosed().subscribe((result) => {
          if (!result) return;

          if (result.action === 'save') {
            if (!this.canSelectUserInModal && this.currentUserId) {
              result.payload.userId = parseInt(this.currentUserId, 10);
            }

            if (this.selectedTenantId) {
              result.payload.tenantId = this.selectedTenantId;
            }

            if (!result.payload.facilityId && currentFacilityId) {
              result.payload.facilityId = currentFacilityId;
            }

            this.appointmentService.create(result.payload).subscribe({
              next: () => {
                this.toastr.success(
                  'Appointment created successfully',
                  'Success'
                );
                this.refreshCalendar();
              },
              error: (err) => {
                console.error('❌ Create error:', err);
                const errorMsg =
                  err.error?.message ||
                  err.error ||
                  'Failed to create appointment';
                this.toastr.error(errorMsg, 'Error');
              },
            });
          }
        });
      },
      error: (err) => {
        this.toastr.error('Failed to load data', 'Error');
      },
    });
  }

  openEditModal(info: any) {
    const appointmentData = info.event.extendedProps;

    const requests: any = {
      facilities: this.facilityService.getAll(),
      appointmentTypes: this.appointmentTypeService.getAll(),
    };

    if (this.canSelectUserInModal) {
      requests.users = this.userService.getUsersForAppointments();
    }

    forkJoin(requests).subscribe({
      next: (data: any) => {
        const dialogRef = this.dialog.open(AppointmentDialogComponent, {
          width: '520px',
          maxWidth: '95vw',
          autoFocus: false,
          disableClose: true,
          hasBackdrop: true,
          restoreFocus: false,
          panelClass: 'appointment-dialog-center',
          data: {
            appointment: {
              id: info.event.id,
              appointmentDateTime: info.event.start,

              facilityId: appointmentData['facilityId'],
              appointmentTypeId: appointmentData['appointmentTypeId'],
              userId: appointmentData['userId'],
              tenantId: appointmentData['tenantId'],
              isFree: appointmentData['isFree'],
              isClosed: appointmentData['isClosed'],
            },
            facilities: data.facilities,
            appointmentTypes: data.appointmentTypes,
            users: data.users || [],
            canSelectUser: this.canSelectUserInModal,
            currentUserId: this.currentUserId,
            tenantId: this.selectedTenantId,
          },
        });

        dialogRef.afterClosed().subscribe((result) => {
          if (!result) {
            return;
          }

          if (result.action === 'save') {
            if (!this.canSelectUserInModal && this.currentUserId) {
              result.payload.userId = parseInt(this.currentUserId, 10);
            }

            if (this.selectedTenantId) {
              result.payload.tenantId = this.selectedTenantId;
            }

            this.appointmentService
              .update(result.payload.id, result.payload)
              .subscribe({
                next: () => {
                  this.toastr.success(
                    'Appointment updated successfully',
                    'Success'
                  );
                  setTimeout(() => this.refreshCalendar(), 100);
                },
                error: (err) => {
                  console.error('❌ Update error:', err);
                  const errorMsg =
                    err.error?.message ||
                    err.error ||
                    'Failed to update appointment';
                  this.toastr.error(errorMsg, 'Error');
                },
              });
          } else if (result.action === 'delete') {
            this.appointmentService.delete(result.id).subscribe({
              next: () => {
                this.toastr.success(
                  'Appointment deleted successfully',
                  'Success'
                );
                setTimeout(() => this.refreshCalendar(), 100);
              },
              error: (err) => {
                const errorMsg =
                  err.error?.message ||
                  err.error ||
                  'Failed to delete appointment';
                this.toastr.error(errorMsg, 'Error');
              },
            });
          }
        });
      },
      error: (err) => {
        this.toastr.error('Failed to load data', 'Error');
      },
    });
  }

  updateAppointmentTime(id: number, newDate: string) {
    const calendarApi = this.calendarComponent.getApi();
    const event = calendarApi.getEventById(id.toString());

    if (!event) {
      this.toastr.error('Event not found', 'Error');
      return;
    }

    const appointmentData = event.extendedProps;

    const oldStart = event.start!;
    const oldEnd = event.end!;

    this.appointmentService
      .update(id, {
        appointmentDateTime: newDate,
        facilityId: appointmentData['facilityId'],
        appointmentTypeId: appointmentData['appointmentTypeId'],
        userId: appointmentData['userId'],
        tenantId: appointmentData['tenantId'],
        isFree: appointmentData['isFree'],
        isClosed: appointmentData['isClosed'],
      })
      .subscribe({
        next: () => {
          this.toastr.success('Appointment time updated', 'Success');
        },
        error: (err) => {
          const errorMsg =
            err.error?.message || err.error || 'Failed to update time';
          this.toastr.error(errorMsg, 'Error');
          event.setDates(oldStart, oldEnd);
        },
      });
  }

  refreshCalendar() {
    if (!this.calendarComponent) return;

    const calendarApi = this.calendarComponent.getApi();
    calendarApi.refetchEvents();
  }
}
