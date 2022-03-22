import { AppContextShape, User, Account } from "../types";

const fakeUser: User = {
  id: 1,
  firstName: "Bob",
  lastName: "Dobalina",
  email: "bdobalina@ucdavis.edu",
  iam: "1000037182",
  kerberos: "bdobalina",
  name: "Mr Mr Mr Bob Dobalina",
  isAdmin: false,
};

const fakeAdminUser: User = {
  id: 1,
  firstName: "Bob",
  lastName: "Dobalina",
  email: "bdobalina@ucdavis.edu",
  iam: "1000037182",
  kerberos: "bdobalina",
  name: "Mr Mr Mr Bob Dobalina",
  isAdmin: true,
};

export const fakeAccounts: Account[] = [
  {
    id: 1,
    name: "Account 1",
    status: "Active",
    canSponsor: true,
    cluster: "caesfarm",
    createdOn: "2020-01-01T00:00:00.000Z",
    updatedOn: "2020-01-01T00:00:00.000Z",
    isAdmin: false,
  },
  {
    id: 2,
    name: "Account 2",
    status: "Active",
    canSponsor: true,
    cluster: "caesfarm",
    createdOn: "2020-01-01T00:00:00.000Z",
    updatedOn: "2020-01-01T00:00:00.000Z",
    isAdmin: false,
  },
];

export const fakeAppContext: AppContextShape = {
  antiForgeryToken: "fakeAntiForgeryToken",
  user: {
    detail: {
      ...fakeUser,
    },
  },
  accounts: [fakeAccounts[0]],
};

export const fakeAdminAppContext: AppContextShape = {
  antiForgeryToken: "fakeAntiForgeryToken",
  user: {
    detail: {
      ...fakeAdminUser,
    },
  },
  accounts: [fakeAccounts[0]],
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
    isAdmin: true,
  },
];

export const fakeAppContextNoAccount: AppContextShape = {
  antiForgeryToken: "fakeAntiForgeryToken",
  user: {
    detail: {
      ...fakeUser,
    },
  },
  accounts: [],
};
