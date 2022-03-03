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

export const fakeAccounts: Account[] = [
  {
    id: 1,
    name: "Account 1",
    status: "Active",
    canSponsor: true,
    createdOn: "2020-01-01T00:00:00.000Z",
  },
  {
    id: 2,
    name: "Account 2",
    status: "Active",
    canSponsor: true,
    createdOn: "2020-01-01T00:00:00.000Z",
  },
];

export const fakeAppContext: AppContextShape = {
  antiForgeryToken: "fakeAntiForgeryToken",
  user: {
    detail: {
      ...fakeUser,
    },
  },
  account: fakeAccounts[0],
};
