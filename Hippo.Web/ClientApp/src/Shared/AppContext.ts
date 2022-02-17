import React from "react";
import { Account, AppContextShape, User } from "../types";

const AppContext = React.createContext<AppContextShape>({
  antiForgeryToken: "",
  user: { detail: {} as User },
  account: {} as Account,
});

export default AppContext;
