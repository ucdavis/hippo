import React from "react";
import {
  AccountModel,
  AppContextShape,
  ClusterModel,
  User,
  Permission,
  RequestModel,
  FeatureFlagsModel,
} from "../types";

const AppContext = React.createContext<
  [AppContextShape, React.Dispatch<React.SetStateAction<AppContextShape>>]
>([
  {
    antiForgeryToken: "",
    user: {
      detail: {} as User,
      permissions: [] as Permission[],
    },
    accounts: [] as AccountModel[],
    clusters: [] as ClusterModel[],
    openRequests: [] as RequestModel[],
    featureFlags: {
      createGroup: false,
      removeAccountFromGroup: false,
    } as FeatureFlagsModel,
  },
  () => {},
]);

export default AppContext;
