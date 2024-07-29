import { useMemo, useState } from "react";
import { Route, Routes, useLocation } from "react-router-dom";

import { AppNav } from "./AppNav";
import AppContext from "./Shared/AppContext";
import { AppContextShape } from "./types";
import BottomSvg from "./Shared/bottomSvg";

import { ClusterHome, Home } from "./components/Home";
import { AccountInfo } from "./components/Account/AccountInfo";
import { RequestForm } from "./components/Account/RequestForm";
import { AccountStatus } from "./components/Account/AccountStatus";
import { Requests } from "./components/Account/Requests";
import { ActiveAccounts } from "./components/Account/ActiveAccounts";
import { ClusterAdmins } from "./components/Admin/ClusterAdmins";
import { ModalProvider } from "react-modal-hook";
import { Toaster } from "react-hot-toast";
import { Clusters } from "./components/Account/Clusters";
import { Clusters as AdminClusters } from "./components/ClusterAdmin/Clusters";
import { Groups } from "./components/Admin/Groups";
import { ShowFor } from "./Shared/ShowFor";
import { FinancialDetail } from "./components/Financial/FinancialDetail";
import { Products } from "./components/Product/Products";
import { Orders } from "./components/Order/Orders";
import { Details } from "./components/Order/Details";
import { CreateOrder } from "./components/Order/CreateOrder";
import { EditOrder } from "./components/Order/EditOrder";
import { UpdateChartStrings } from "./components/Order/UpdateChartStrings";
import { SoftwareRequestForm } from "./components/ClusterAdmin/SoftwareRequestForm";
import NotFound from "./NotFound";
import { OrderHistories } from "./components/Order/OrderHistories";
import { OrderPayments } from "./components/Order/OrderPayments";

declare var Hippo: AppContextShape;

const Restricted = () => (
  <div>Sorry, you don't have access to see this page</div>
);

const App = () => {
  const [context, setContext] = useState<AppContextShape>(Hippo);

  const loc = useLocation();

  const accountClassName = useMemo(
    () => loc.pathname.replace("/", ""),
    [loc.pathname],
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
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/clusters" element={<Clusters />} />
            <Route path="/:cluster" element={<ClusterHome />} />
            <Route path="/:cluster/myaccount" element={<AccountInfo />} />
            <Route path="/:cluster/accountstatus" element={<AccountStatus />} />
            <Route path="/:cluster/create" element={<RequestForm />} />
            <Route
              path="/:cluster/approve"
              element={
                <ShowFor roles={["GroupAdmin"]} alternative={<Restricted />}>
                  <Requests />
                </ShowFor>
              }
            />
            <Route
              path="/:cluster/activeaccounts"
              element={
                <ShowFor roles={["GroupAdmin"]} alternative={<Restricted />}>
                  <ActiveAccounts />
                </ShowFor>
              }
            />
            <Route
              path="/:cluster/admin/groups"
              element={
                <ShowFor roles={["ClusterAdmin"]} alternative={<Restricted />}>
                  <Groups />
                </ShowFor>
              }
            />
            <Route
              path="/:cluster/admin/clusteradmins"
              element={
                <ShowFor roles={["ClusterAdmin"]} alternative={<Restricted />}>
                  <ClusterAdmins />
                </ShowFor>
              }
            />
            <Route
              path="/clusteradmin/clusters"
              element={
                <ShowFor roles={["System"]} alternative={<Restricted />}>
                  <AdminClusters />
                </ShowFor>
              }
            />
            <Route
              path="/:cluster/admin/financialdetails"
              element={
                <ShowFor roles={["System"]} alternative={<Restricted />}>
                  <FinancialDetail />
                </ShowFor>
              }
            />
            <Route
              path="/:cluster/product/index"
              element={
                <ShowFor
                  roles={["System", "ClusterAdmin", "GroupAdmin"]}
                  alternative={<Restricted />}
                >
                  <Products />
                </ShowFor>
              }
            />
            <Route
              path="/:cluster/order/:orderType"
              element={
                <ShowFor
                  roles={["System", "ClusterAdmin", "GroupAdmin"]}
                  alternative={<Restricted />}
                >
                  <Orders />
                </ShowFor>
              }
            />
            <Route
              path="/:cluster/order/details/:orderId"
              element={
                <ShowFor
                  roles={["System", "ClusterAdmin", "GroupAdmin"]}
                  alternative={<Restricted />}
                >
                  <Details />
                </ShowFor>
              }
            />
            <Route
              path="/:cluster/order/orderHistories/:orderId"
              element={
                <ShowFor
                  roles={["System", "ClusterAdmin", "GroupAdmin"]}
                  alternative={<Restricted />}
                >
                  <OrderHistories />
                </ShowFor>
              }
            />
            <Route
              path="/:cluster/order/orderPayments/:orderId"
              element={
                <ShowFor
                  roles={["System", "ClusterAdmin", "GroupAdmin"]}
                  alternative={<Restricted />}
                >
                  <OrderPayments />
                </ShowFor>
              }
            />
            <Route
              path="/:cluster/order/edit/:orderId"
              element={
                <ShowFor
                  roles={["System", "ClusterAdmin", "GroupAdmin"]}
                  alternative={<Restricted />}
                >
                  <EditOrder />
                </ShowFor>
              }
            />
            <Route
              path="/:cluster/order/updatechartstrings/:orderId"
              element={
                <ShowFor
                  roles={["System", "ClusterAdmin", "GroupAdmin"]}
                  alternative={<Restricted />}
                >
                  <UpdateChartStrings />
                </ShowFor>
              }
            />
            <Route
              path="/:cluster/order/create/:productId?"
              element={
                <ShowFor
                  roles={["System", "ClusterAdmin", "GroupAdmin"]}
                  alternative={<Restricted />}
                >
                  <CreateOrder />
                </ShowFor>
              }
            />
            <Route path="/softwarerequest" element={<SoftwareRequestForm />} />
            <Route path="*" element={<NotFound />} />
          </Routes>
        </div>
      </ModalProvider>
    </AppContext.Provider>
  );
};

export default App;
