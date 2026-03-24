import { environment } from 'src/environments/environment';

class Logger {
  log(...args: unknown[]): void {
    if (!environment.production) {
      console.log(...args);
    }
  }

  error(...args: unknown[]): void {
    if (!environment.production) {
      console.error(...args);
    }
  }

  warn(...args: unknown[]): void {
    if (!environment.production) {
      console.warn(...args);
    }
  }

  info(...args: unknown[]): void {
    if (!environment.production) {
      console.info(...args);
    }
  }
}

export const logger = new Logger();
