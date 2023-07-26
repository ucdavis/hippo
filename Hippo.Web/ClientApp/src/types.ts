export interface User {
  id: number;
  firstName?: string;
  lastName: string;
  email: string;
  iam: string;
  kerberos: string;
  name: string;
}

export type RoleName = "System" | "ClusterAdmin" | "GroupAdmin" | "GroupMember";

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
  groupId: number;
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
    permissions: Permission[];
  };
  accounts: Account[];
  clusters: Cluster[];
}

export interface Permission {
  role: RoleName;
  cluster?: string;
  group?: string;
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
