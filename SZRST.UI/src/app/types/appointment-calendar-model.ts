export interface AppointmentCalendar {
  id: number;
  start: string;
  end: string;
  isFree: boolean;
  isClosed: boolean;
  facilityId: number;
  facilityName: string;
  appointmentTypeId: number;
  appointmentTypeName: string;
  price: number;
  userId?: string;
  tenantId?: number;
}