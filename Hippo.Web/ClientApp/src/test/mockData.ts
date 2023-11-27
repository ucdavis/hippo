import {
  AppContextShape,
  User,
  AccountModel,
  GroupModel,
  Cluster,
  RequestModel,
} from "../types";

const fakeUser: User = {
  id: 1,
  firstName: "Bob",
  lastName: "Dobalina",
  email: "bdobalina@ucdavis.edu",
  iam: "1000037182",
  kerberos: "bdobalina",
  name: "Mr Mr Mr Bob Dobalina",
};

const fakeAdminUser: User = {
  id: 1,
  firstName: "Bob",
  lastName: "Dobalina",
  email: "bdobalina@ucdavis.edu",
  iam: "1000037182",
  kerberos: "bdobalina",
  name: "Mr Mr Mr Bob Dobalina",
};

export const fakeGroups: GroupModel[] = [
  {
    id: 1,
    name: "group1",
    displayName: "Group 1",
    admins: [],
  },
  {
    id: 2,
    name: "group2",
    displayName: "Group 2",
    admins: [],
  },
];

export const fakeAccounts: AccountModel[] = [
  {
    id: 1,
    name: "Account 1",
    email: fakeUser.email,
    cluster: "caesfarm",
    createdOn: "2020-01-01T00:00:00.000Z",
    updatedOn: "2020-01-01T00:00:00.000Z",
    memberOfGroups: [fakeGroups[0]],
    adminOfGroups: [],
  },
  {
    id: 2,
    name: "Account 2",
    email: fakeUser.email,
    cluster: "caesfarm",
    createdOn: "2020-01-01T00:00:00.000Z",
    updatedOn: "2020-01-01T00:00:00.000Z",
    memberOfGroups: [fakeGroups[1]],
    adminOfGroups: [],
  },
];

export const fakeRequests: RequestModel[] = [
  {
    id: 1,
    requesterEmail: fakeUser.email,
    requesterName: fakeUser.name,
    action: "CreateAccount",
    groupModel: fakeGroups[0],
    status: "PendingApproval",
    cluster: "caesfarm",
    supervisingPI: "Dr. Bob Dobalina",
  },
  {
    id: 2,
    requesterEmail: fakeUser.email,
    requesterName: fakeUser.name,
    action: "AddAccountToGroup",
    groupModel: fakeGroups[1],
    status: "PendingApproval",
    cluster: "caesfarm",
    supervisingPI: "Dr. Bob Dobalina",
  },
];

const fakeCluster: Cluster = {
  id: 1,
  name: "caesfarm",
  description: "The farm cluster",
  sshKeyId: "90775ee7-1117-43ce-a02a-d335075e040d",
  sshName: "ssh-name",
  sshUrl: "ssh-url.com",
  domain: "repo/yaml/path.yml",
};

export const fakeAppContext: AppContextShape = {
  antiForgeryToken: "fakeAntiForgeryToken",
  user: {
    detail: {
      ...fakeUser,
    },
    permissions: [
      {
        role: "GroupMember",
        cluster: "caesfarm",
      },
    ],
  },
  accounts: [fakeAccounts[0]],
  clusters: [fakeCluster],
  openRequests: fakeRequests,
};

export const fakeGroupAdminAppContext: AppContextShape = {
  antiForgeryToken: "fakeAntiForgeryToken",
  user: {
    detail: {
      ...fakeAdminUser,
    },
    permissions: [
      {
        role: "GroupAdmin",
        cluster: "caesfarm",
      },
    ],
  },
  accounts: [fakeAccounts[0]],
  clusters: [fakeCluster],
  openRequests: fakeRequests,
};

export const fakeAdminUsers: User[] = [
  fakeAdminUser,
  {
    id: 4,
    firstName: "Bobby",
    lastName: "Dob",
    email: "bdob@ucdavis.edu",
    iam: "1000037199",
    kerberos: "bdob",
    name: "A Fake Admin User",
  },
];

export const fakeAppContextNoAccount: AppContextShape = {
  antiForgeryToken: "fakeAntiForgeryToken",
  user: {
    detail: {
      ...fakeUser,
    },
    permissions: [],
  },
  accounts: [],
  clusters: [fakeCluster],
  openRequests: [],
};
