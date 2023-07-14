export interface User {
  id: number;
  firstName?: string;
  lastName: string;
  email: string;
  iam: string;
  kerberos: string;
  name: string;
  isAdmin: boolean;
}

export type RoleName = "Admin" | "Sponsor" | "System";

export interface Account {
  id: number;
  name: string;
  status: string;
  canSponsor: boolean;
  createdOn: string;
  cluster: string;
  owner?: User;
  sponsor?: Account;
  updatedOn: string;
  isAdmin: boolean;
}

export interface Cluster {
  id: number;
  name: string;
  description: string;
  sshName: string;
  sshKeyId: string;
  sshUrl: string;
  domain: string;
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
  accounts: Account[];
  clusters: Cluster[];
}

export interface PromiseStatus {
  pending: boolean;
  success: boolean;
}

export interface IRouteParams {
  cluster: string;
}

export interface ClusterModel {
  cluster: Cluster;
  sshKey?: string;
}
