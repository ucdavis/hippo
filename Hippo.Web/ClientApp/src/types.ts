export interface User {
  id: number;
  firstName?: string;
  lastName: string;
  email: string;
  iam: string;
  kerberos: string;
  name: string;
  IsAdmin: boolean;
}

export type RoleName = "Admin" | "Sponsor";

export interface Account {
  id: number;
  name: string;
  status: string;
  canSponsor: boolean;
  createdOn: string;
  owner?: User;
  sponsor?: Account;
}

export interface RequestPostModel {
  sponsorId: number;
  sshKey: string;
}

export interface CreateSponsorPostModel {
  lookup: string;
  name: string;
}

export interface AppContextShape {
  antiForgeryToken: string;
  user: {
    detail: User;
  };
  account: Account;
}

export interface PromiseStatus {
  pending: boolean;
  success: boolean;
}
