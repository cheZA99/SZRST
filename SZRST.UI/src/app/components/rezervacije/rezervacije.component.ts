import { Component, OnInit } from '@angular/core';
import { CalendarOptions, EventInput } from '@fullcalendar/core';
import timeGridPlugin from '@fullcalendar/timegrid';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';
import { AppointmentService } from '../../services/appointment.service';
import { AppointmentCalendar } from 'src/app/types/appointment-calendar-model';
import { ViewChild } from '@angular/core';
import { FullCalendarComponent } from '@fullcalendar/angular';
import locale from '@fullcalendar/core/locales/hr';
import { MatDialog } from '@angular/material/dialog';
import { AppointmentDialogComponent } from '../rezervacije-dialog/rezervacije-dialog.component';
import { forkJoin, of } from 'rxjs';
import { FacilityService } from 'src/app/services/facility.service';
import { AppointmentTypeService } from 'src/app/services/appointment-type.service';
import { TenantService, Tenant } from 'src/app/services/tenant.service';
import { UserService, User } from 'src/app/services/user.service';
import { AuthService } from 'src/app/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute, Router } from '@angular/router';
import { logger } from 'src/app/utils/logger';

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
  selectedTenantName: string = '';
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

  fromDashboard = false;
  initialLoadComplete = false;

  calendarOptions: CalendarOptions = {
    locale: locale,
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
    private toastr: ToastrService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

ngOnInit() {
  this.initializeUserData();
  
  const params = this.route.snapshot.queryParams;
  logger.log('Initial query params:', params);
  
  this.fromDashboard = params['fromDashboard'] === 'true';
  
  if (params['tenantId']) {
    const tenantId = +params['tenantId'];
    if (tenantId && !isNaN(tenantId)) {
      this.selectedTenantId = tenantId;
      
      if (params['tenantName']) {
        this.selectedTenantName = decodeURIComponent(params['tenantName']);
      }
    }
  }
  
  if (params['facilityId']) {
    const facilityId = +params['facilityId'];
    if (facilityId && !isNaN(facilityId)) {
      this.selectedFacilityId = facilityId;
    }
  }
  
  if (!this.selectedTenantId && (this.isAdmin || this.isUposlenik)) {
    this.selectedTenantId = this.tenantIdFromToken;
    if (this.selectedTenantId) {
      this.loadTenantName(this.selectedTenantId);
    }
  }
  
  this.loadAllData();
}


loadAllData(): void {
  forkJoin({
    tenants: this.showTenantFilter ? this.tenantService.getAllTenants() : of([]),
    facilities: this.facilityService.getAll()
  }).subscribe({
    next: (data) => {
      this.tenants = data.tenants;
      this.facilities = data.facilities.items;
      
      logger.log('Data loaded:', {
        tenants: this.tenants.length,
        facilities: this.facilities.length,
        selectedTenantId: this.selectedTenantId,
        selectedFacilityId: this.selectedFacilityId
      });
      
      this.applyFacilityFilter();
      
      if (!this.selectedTenantId && this.showTenantFilter && this.tenants.length > 0) {
        this.selectedTenantId = this.tenants[0].id;
        this.selectedTenantName = this.tenants[0].name;
        this.applyFacilityFilter();
      }
      
      if (!this.selectedTenantId && (this.isAdmin || this.isUposlenik) && this.tenantIdFromToken) {
        this.selectedTenantId = this.tenantIdFromToken;
        this.loadTenantName(this.selectedTenantId);
        this.applyFacilityFilter();
      }
      
      if (this.selectedFacilityId && this.filteredFacilities.length > 0) {
        const facilityExists = this.filteredFacilities.some(f => f.id === this.selectedFacilityId);
        if (!facilityExists) {
          const facilityFromAll = this.facilities.find(f => f.id === this.selectedFacilityId);
          if (facilityFromAll) {
            this.selectedTenantId = facilityFromAll.tenantId;
            const tenant = this.tenants.find(t => t.id === this.selectedTenantId);
            if (tenant) {
              this.selectedTenantName = tenant.name;
            }
            this.applyFacilityFilter();
          }
        }
      }
      
      if (!this.selectedFacilityId && this.filteredFacilities.length > 0) {
        this.selectedFacilityId = this.filteredFacilities[0].id;
        logger.log('Setting facility to first:', this.selectedFacilityId);
      }
      
      if (this.selectedFacilityId) {
        logger.log('Ready to show calendar. Tenant:', this.selectedTenantId, 'Facility:', this.selectedFacilityId);
        setTimeout(() => this.refreshCalendar(), 300);
      }
      
      this.initialLoadComplete = true;
    },
    error: (err) => {
      logger.error('Failed to load data:', err);
      this.toastr.error('Failed to load data', 'Error');
    }
  });
}

applyQueryParams(): void {
  const params = this.route.snapshot.queryParams;
  
  if (params['tenantId']) {
    const tenantId = +params['tenantId'];
    this.selectedTenantId = tenantId;
    
    if (params['tenantName']) {
      this.selectedTenantName = decodeURIComponent(params['tenantName']);
    } else if (tenantId) {
      const tenant = this.tenants.find(t => t.id === tenantId);
      this.selectedTenantName = tenant ? tenant.name : '';
    }
  } else if (this.showTenantFilter && this.tenants.length > 0) {
    this.selectedTenantId = this.tenants[0].id;
    this.selectedTenantName = this.tenants[0].name;
  }
  
  this.applyFacilityFilter();
  
  if (params['facilityId']) {
    const facilityId = +params['facilityId'];
    
    const facilityExists = this.filteredFacilities.some(f => f.id === facilityId);
    if (facilityExists) {
      this.selectedFacilityId = facilityId;
    } else if (this.filteredFacilities.length > 0) {
      this.selectedFacilityId = this.filteredFacilities[0].id;
    }
  } else if (this.filteredFacilities.length > 0) {
    this.selectedFacilityId = this.filteredFacilities[0].id;
  }
  
  if (this.selectedFacilityId) {
    setTimeout(() => this.refreshCalendar(), 300);
  }
  
  this.initialLoadComplete = true;
}

  loadTenantName(tenantId: number): void {
    this.tenantService.getTenantById(tenantId).subscribe({
      next: (tenant) => {
        this.selectedTenantName = tenant.name;
      },
      error: (err) => {
        logger.error('Error loading tenant name:', err);
      }
    });
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
      this.canSelectUserInModal = this.isAdmin || this.isUposlenik || this.isSuperAdmin;

      if ((this.isAdmin || this.isUposlenik) && decoded.tenantId) {
        this.tenantIdFromToken = +decoded.tenantId;
        if (!this.selectedTenantId) {
          this.selectedTenantId = this.tenantIdFromToken;
          this.loadTenantName(this.tenantIdFromToken);
        }
      }
    }
  }

  loadFacilities() {
    this.facilityService.getAll().subscribe({
      next: (data) => {
        this.facilities = data.items;
        this.applyFacilityFilter();
        
        if (this.selectedFacilityId) {
          const facilityExists = this.filteredFacilities.some(f => f.id === this.selectedFacilityId);
          if (!facilityExists && this.filteredFacilities.length > 0) {
            this.selectedFacilityId = this.filteredFacilities[0].id;
          }
        }
        
        if (!this.selectedFacilityId && this.filteredFacilities.length > 0) {
          this.selectedFacilityId = this.filteredFacilities[0].id;
        }
        
        if (this.selectedTenantId && this.selectedFacilityId) {
          setTimeout(() => this.refreshCalendar(), 300);
        }
      },
      error: (err) => {
        logger.error('Failed to load facilities:', err);
        this.toastr.error('Failed to load facilities', 'Error');
      },
    });
  }

  loadTenants() {
    this.tenantService.getAllTenants().subscribe({
      next: (data) => {
        this.tenants = data;
        
        if (this.selectedTenantId) {
          const tenantExists = data.some(t => t.id === this.selectedTenantId);
          if (!tenantExists && data.length > 0) {
            this.selectedTenantId = data[0].id;
            this.selectedTenantName = data[0].name;
            this.applyFacilityFilter();
          }
        } else if (data.length > 0 && this.showTenantFilter) {
          this.selectedTenantId = data[0].id;
          this.selectedTenantName = data[0].name;
          this.applyFacilityFilter();
        }
      },
      error: (err) => {
        logger.error('Failed to load organizations:', err);
        this.toastr.error('Failed to load organizations', 'Error');
      },
    });
  }

applyFacilityFilter() {
  logger.log('Applying facility filter for tenant:', this.selectedTenantId);
  
  if (this.selectedTenantId) {
    this.filteredFacilities = this.facilities.filter(
      (f) => f.tenantId === this.selectedTenantId
    );
    logger.log('Filtered facilities for tenant', this.selectedTenantId, ':', this.filteredFacilities.length);
  } else {
    this.filteredFacilities = [];
    logger.log('No tenant selected, filtered facilities cleared');
  }
}
  loadAppointments(
    startStr: string,
    endStr: string,
    successCallback: any,
    failureCallback: any
  ) {
    if (!this.selectedFacilityId) {
      successCallback([]);
      return;
    }

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
          logger.error('Failed to load appointments:', err);
          this.toastr.error('Failed to load appointments', 'Error');
          failureCallback(err);
        },
      });
  }

   get canShowCalendar(): boolean {
    return !!this.selectedFacilityId;
  }

onFacilityFilterChange(event: Event) {
  const select = event.target as HTMLSelectElement;
  const value = select.value;
  this.selectedFacilityId = value ? parseInt(value, 10) : null;
  logger.log('Facility changed to:', this.selectedFacilityId);
  
  this.updateUrlWithSelectedFilters();
  
  this.refreshCalendar();
}


onTenantFilterChange(event: Event) {
  const select = event.target as HTMLSelectElement;
  const value = select.value;
  this.selectedTenantId = value ? parseInt(value, 10) : null;
  
  logger.log('Tenant changed to:', this.selectedTenantId);
  
  if (this.selectedTenantId) {
    const selectedTenant = this.tenants.find(t => t.id === this.selectedTenantId);
    this.selectedTenantName = selectedTenant ? selectedTenant.name : '';
  } else {
    this.selectedTenantName = '';
  }

  this.applyFacilityFilter();
  
  this.selectedFacilityId = null;
  
  this.updateUrlWithSelectedFilters();
  
  this.refreshCalendar();
}


updateUrlWithSelectedFilters(): void {
  const queryParams: any = {
    fromDashboard: 'false',
    autoSelect: 'true'
  };

  if (this.selectedTenantId) {
    queryParams.tenantId = this.selectedTenantId;
    
    const tenant = this.tenants.find(t => t.id === this.selectedTenantId);
    if (tenant) {
      queryParams.tenantName = encodeURIComponent(tenant.name);
    } else {
      queryParams.tenantName = encodeURIComponent(this.selectedTenantName);
    }
  }

  if (this.selectedFacilityId) {
    queryParams.facilityId = this.selectedFacilityId;
    
    const facility = this.filteredFacilities.find(f => f.id === this.selectedFacilityId) || 
                     this.facilities.find(f => f.id === this.selectedFacilityId);
    if (facility) {
      queryParams.facilityName = encodeURIComponent(facility.name);
    }
  }

  logger.log('Updating URL with params:', queryParams);
  
  this.router.navigate([], {
    relativeTo: this.route,
    queryParams: queryParams,
    queryParamsHandling: 'merge',
    replaceUrl: true
  });
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

    if (this.canSelectUserInModal && this.selectedTenantId) {
      requests.users = this.userService.getUsersForAppointments(this.selectedTenantId);
    }

    forkJoin(requests).subscribe({
      next: (data: any) => {
        let filteredFacilities = data.facilities.items;
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
                logger.error('❌ Create error:', err);
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

  getSelectedFacilityName(): string {
  if (!this.selectedFacilityId || !this.filteredFacilities.length) {
    return '...';
  }
  
  const facility = this.filteredFacilities.find(f => f.id === this.selectedFacilityId);
  return facility ? facility.name : '...';
}

  openEditModal(info: any) {
    const appointmentData = info.event.extendedProps;

    const requests: any = {
      facilities: this.facilityService.getAll(),
      appointmentTypes: this.appointmentTypeService.getAll(),
    };

    const appointmentTenantId = appointmentData['tenantId']
      ? Number(appointmentData['tenantId'])
      : this.selectedTenantId;

    if (this.canSelectUserInModal && appointmentTenantId) {
      requests.users = this.userService.getUsersForAppointments(appointmentTenantId);
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
            facilities: data.facilities.items,
            appointmentTypes: data.appointmentTypes,
            users: data.users || [],
            canSelectUser: this.canSelectUserInModal,
            currentUserId: this.currentUserId,
            tenantId: appointmentTenantId,
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
                  logger.error('❌ Update error:', err);
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
    if (!this.calendarComponent) {
      return;
    }
    const calendarApi = this.calendarComponent.getApi();
    calendarApi.refetchEvents();
  }
}
