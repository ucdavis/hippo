
export interface User {
  id: number;
  firstName?: string;
  lastName: string;
  email: string;
  iam: string;
  kerberos: string;
  name: string;
}

export interface AppContextShape {
  antiForgeryToken: string;
  user: {
    detail: User;
  };
}
