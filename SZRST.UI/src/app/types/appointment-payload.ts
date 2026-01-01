export interface AppointmentPayload {
  id?: number;
  appointmentDateTime: string;
  isFree: boolean;
  isClosed: boolean;
  facilityId: number;
  appointmentTypeId: number;
}
