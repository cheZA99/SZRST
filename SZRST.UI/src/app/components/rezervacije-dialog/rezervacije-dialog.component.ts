import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { User } from 'src/app/services/user.service';

@Component({
  selector: 'app-appointment-dialog',
  templateUrl: './rezervacije-dialog.component.html',
  styleUrls: ['./rezervacije-dialog.component.css'],
})
export class AppointmentDialogComponent implements OnInit {
  isEdit = false;

  facilities: any[] = [];
  appointmentTypes: any[] = [];
  users: User[] = [];
  times: string[] = [];

  selectedDate: string = '';
  selectedTime: string = '';
  selectedFacilityId: number | null = null;
  selectedTypeId: number | null = null;
  selectedUserId: string | null = null;
  selectedTenantId: number | null = null;
  isFree: boolean = true;
  isClosed: boolean = false;

  canSelectUser = false;

  constructor(
    private dialogRef: MatDialogRef<AppointmentDialogComponent>,
    @Inject(MAT_DIALOG_DATA)
    public data: {
      appointment?: any;
      facilities: any[];
      appointmentTypes: any[];
      users: User[];
      initialDate?: Date;
      canSelectUser: boolean;
      currentUserId: string | null;
      tenantId?: number;
    }
  ) {
    this.dialogRef.backdropClick().subscribe(() => {
      this.cancel();
    });

    this.dialogRef.keydownEvents().subscribe((event) => {
      if (event.key === 'Escape') {
        this.cancel();
      }
    });
  }

  ngOnInit() {
    this.buildTimes();
    this.isEdit = !!this.data?.appointment;

    let targetTenantId: number | null = null;

    if (this.isEdit && this.data?.appointment?.tenantId) {
      targetTenantId = this.data.appointment.tenantId;
    } else if (this.data?.tenantId) {
      targetTenantId = this.data.tenantId;
    }

    this.facilities = this.data.facilities || [];
    this.appointmentTypes = this.data.appointmentTypes || [];
    this.users = this.data.users || [];
    this.canSelectUser = this.data.canSelectUser || false;

    let initialDate: Date;

    if (this.data?.appointment?.appointmentDateTime) {
      const dateStr = this.data.appointment.appointmentDateTime;
      initialDate = new Date(dateStr);
    } else if (this.data?.initialDate) {
      initialDate = new Date(this.data.initialDate);
    } else {
      initialDate = new Date();
    }

    this.selectedDate = this.formatDate(initialDate);
    this.selectedTime = this.formatTime(initialDate);

    if (this.isEdit && this.data?.appointment) {

      this.selectedFacilityId =
        Number(this.data.appointment.facilityId) || null;
      this.selectedTypeId =
        Number(this.data.appointment.appointmentTypeId) || null;
      this.selectedUserId = this.data.appointment.userId?.toString() || null;
      this.selectedTenantId = Number(this.data.appointment.tenantId) || null;
      this.isFree = this.data.appointment.isFree ?? true;
      this.isClosed = this.data.appointment.isClosed ?? false;

      if (this.selectedTenantId) {
        this.facilities = this.facilities.filter(
          (f) => f.tenantId === this.selectedTenantId
        );
        this.appointmentTypes = this.appointmentTypes.filter(
          (t) => t.tenantId === this.selectedTenantId
        );
      }
    } else {
      if (!this.canSelectUser && this.data.currentUserId) {
        this.selectedUserId = this.data.currentUserId;
      }
      this.selectedTenantId = targetTenantId;

      if (this.selectedTenantId) {
        this.facilities = this.facilities.filter(
          (f) => f.tenantId === this.selectedTenantId
        );
        this.appointmentTypes = this.appointmentTypes.filter(
          (t) => t.tenantId === this.selectedTenantId
        );
      }
    }
  }

  buildTimes() {
    for (let h = 7; h <= 19; h++) {
      for (let m of [0, 30]) {
        this.times.push(
          `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}`
        );
      }
    }
  }

  formatDate(date: Date): string {
    const year = date.getFullYear();
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const day = date.getDate().toString().padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  formatTime(date: Date): string {
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${hours}:${minutes}`;
  }

  onDateInput(event: Event) {
    const input = event.target as HTMLInputElement;
    this.selectedDate = input.value;
  }

  onTimeChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    this.selectedTime = select.value;
  }

  onFacilityChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const value = select.value;
    this.selectedFacilityId = value ? parseInt(value, 10) : null;
  }

  onTypeChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const value = select.value;
    this.selectedTypeId = value ? parseInt(value, 10) : null;
  }

  onUserChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const value = select.value;
    this.selectedUserId = value ? value : null;
  }

  onFreeChange(event: Event) {
    const checkbox = event.target as HTMLInputElement;
    this.isFree = checkbox.checked;
  }

  onClosedChange(event: Event) {
    const checkbox = event.target as HTMLInputElement;
    this.isClosed = checkbox.checked;
  }

  isFormValid(): boolean {
    const hasFacility =
      this.selectedFacilityId !== null &&
      this.selectedFacilityId !== undefined &&
      !isNaN(this.selectedFacilityId);

    const hasType =
      this.selectedTypeId !== null &&
      this.selectedTypeId !== undefined &&
      !isNaN(this.selectedTypeId);

    let hasUser = true;
    if (this.canSelectUser) {
      hasUser =
        this.selectedUserId !== null &&
        this.selectedUserId !== undefined &&
        this.selectedUserId !== '';
    }

    return !!(
      this.selectedDate &&
      this.selectedTime &&
      hasFacility &&
      hasType &&
      hasUser
    );
  }

  save() {
    if (!this.isFormValid()) {
      console.error('Form invalid:', {
        date: this.selectedDate,
        time: this.selectedTime,
        facilityId: this.selectedFacilityId,
        typeId: this.selectedTypeId,
        userId: this.selectedUserId,
      });
      return;
    }

    const [h, m] = this.selectedTime.split(':').map(Number);

    const dateStr = this.selectedDate;
    const timeStr = `${h.toString().padStart(2, '0')}:${m
      .toString()
      .padStart(2, '0')}:00`;
    const localDateTimeStr = `${dateStr}T${timeStr}`;

    const userIdAsInt = this.selectedUserId
      ? parseInt(this.selectedUserId, 10)
      : null;

    const payload: any = {
      appointmentDateTime: localDateTimeStr,
      facilityId: this.selectedFacilityId!,
      appointmentTypeId: this.selectedTypeId!,
      userId: userIdAsInt!,
      tenantId: this.selectedTenantId,
      isFree: this.isFree,
      isClosed: this.isClosed,
    };

    if (this.isEdit && this.data?.appointment?.id) {
      payload.id = this.data.appointment.id;
    }

    this.dialogRef.close({ action: 'save', payload });
  }

  delete() {
    if (!this.data?.appointment?.id) return;

    if (confirm('Are you sure you want to delete this appointment?')) {
      this.dialogRef.close({
        action: 'delete',
        id: this.data.appointment.id,
      });
    }
  }

  cancel() {
    this.dialogRef.close(null);
  }
}
