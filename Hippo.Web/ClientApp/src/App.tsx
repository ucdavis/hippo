import { useMemo, useState } from "react";
import { Route, Switch, useLocation } from "react-router-dom";

import { AppNav } from "./AppNav";
import AppContext from "./Shared/AppContext";
import { AppContextShape } from "./types";
import BottomSvg from "./Shared/bottomSvg";

import { ClusterHome, Home } from "./components/Home";
import { AccountInfo } from "./components/Account/AccountInfo";
import { RequestForm } from "./components/Account/RequestForm";
import { PendingApproval } from "./components/Account/PendingApproval";
import { ApproveAccounts } from "./components/Account/ApproveAccounts";
import { ActiveAccounts } from "./components/Account/ActiveAccounts";
import { ClusterAdmins } from "./components/Admin/ClusterAdmins";
import { GroupAdmins } from "./components/Admin/GroupAdmins";
import { ConditionalRoute } from "./ConditionalRoute";
import { ModalProvider } from "react-modal-hook";
import { Toaster } from "react-hot-toast";
import { Clusters } from "./components/Account/Clusters";
import { Clusters as AdminClusters } from "./components/ClusterAdmin/Clusters";

declare var Hippo: AppContextShape;

const App = () => {
  const [context, setContext] = useState<AppContextShape>(Hippo);

  const loc = useLocation();

  const accountClassName = useMemo(
    () => loc.pathname.replace("/", ""),
    [loc.pathname]
  );

  return (
    <AppContext.Provider value={[context, setContext]}>
      <ModalProvider>
        <Toaster />
        <div className={`account-status-${accountClassName}`}>
          <AppNav></AppNav>
          <div className="top-svg">
            <BottomSvg />
          </div>
          <Switch>
            <Route exact path="/" component={Home} />
            <Route path="/clusters" component={Clusters} />
            <Route exact path="/:cluster" component={ClusterHome} />
            <Route path="/:cluster/active" component={AccountInfo} />
            <Route
              path="/:cluster/pendingapproval"
              component={PendingApproval}
            />
            <Route path="/:cluster/create" component={RequestForm} />
            <ConditionalRoute
              roles={["GroupAdmin"]}
              path="/:cluster/approve"
              component={ApproveAccounts}
            />
            <ConditionalRoute
              roles={["GroupAdmin"]}
              path="/:cluster/activeaccounts"
              component={ActiveAccounts}
            />
            <ConditionalRoute
              roles={["ClusterAdmin"]}
              path="/:cluster/admin/clusteradmins"
              component={ClusterAdmins}
            />
            <ConditionalRoute
              roles={["ClusterAdmin"]}
              path="/:cluster/admin/groupadmins"
              component={GroupAdmins}
            />
            <ConditionalRoute
              roles={["System"]}
              path="/clusteradmin/clusters"
              component={AdminClusters}
            />
          </Switch>
        </div>
      </ModalProvider>
    </AppContext.Provider>
  );
};

export default App;
