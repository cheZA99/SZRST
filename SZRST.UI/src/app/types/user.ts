export type User = {
  userName: string;
  message: string;
  accessToken: string;
  refreshToken: string;
  accessTokenExpires?: Date;
  roles: string[];
  tenantId?: number;
};
