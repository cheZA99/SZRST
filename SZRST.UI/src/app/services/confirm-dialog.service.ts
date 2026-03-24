import { Injectable } from '@angular/core';
import Swal, { SweetAlertIcon } from 'sweetalert2';

export interface ConfirmDialogOptions {
  title: string;
  text?: string;
  icon?: SweetAlertIcon;
  confirmButtonText?: string;
  cancelButtonText?: string;
  confirmButtonColor?: string;
}

@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  async confirm(options: ConfirmDialogOptions): Promise<boolean> {
    const result = await Swal.fire({
      title: options.title,
      text: options.text,
      icon: options.icon ?? 'warning',
      showCancelButton: true,
      confirmButtonText: options.confirmButtonText ?? 'Potvrdi',
      cancelButtonText: options.cancelButtonText ?? 'Otkaži',
      confirmButtonColor: options.confirmButtonColor ?? '#d33'
    });

    return result.isConfirmed;
  }
}
