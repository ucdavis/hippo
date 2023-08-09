import { useContext } from "react";
import { NavLink, useRouteMatch } from "react-router-dom";
import AppContext from "./Shared/AppContext";
import HippoLogo from "./Shared/hippoLogo";
import { ShowFor } from "./Shared/ShowFor";
import { IRouteParams } from "./types";

export const AppNav = () => {
  const [{ clusters }] = useContext(AppContext);
  const match = useRouteMatch<IRouteParams>("/:cluster/:path");
  const cluster = clusters.find((c) => c.name === match?.params.cluster);

  return (
    <div>
      <div className="row appheader justify-content-center">
        <div className="col-md-8 hippo">
          <NavLink to={cluster ? `/${cluster?.name}/` : "/clusters"}>
            <HippoLogo />
          </NavLink>
          <h1>
            <img src="/media/ucdavis.svg" alt="UC DAVIS" />
            HiPPO
          </h1>
          <p className="lede">High Performance Personnel Onboarding</p>
          {cluster && (
            <p>
              <strong>{cluster.description}</strong> |{" "}
              <NavLink to="/multiple">Change Cluster</NavLink>
            </p>
          )}
        </div>
      </div>
      {/* Only show links if a cluster has been identified */}
      {cluster && (
        <div className="row justify-content-center">
          <div className="col-md-8">
            <nav className="simple-nav">
              <ShowFor roles={["GroupAdmin"]}>
                <NavLink
                  id="sponsorApprove"
                  to={`/${cluster.name}/approve`}
                  className="nav-item nav-link"
                  activeStyle={{
                    fontWeight: "bold",
                  }}
                >
                  Pending Approvals
                </NavLink>
                <NavLink
                  id="activeAccounts"
                  to={`/${cluster.name}/activeaccounts`}
                  className="nav-item nav-link"
                  activeStyle={{
                    fontWeight: "bold",
                  }}
                >
                  Active Accounts
                </NavLink>
              </ShowFor>
              <ShowFor roles={["ClusterAdmin"]}>
                <NavLink
                  id="AdminIndex"
                  className="nav-item nav-link"
                  to={`/${cluster.name}/admin/users`}
                  activeStyle={{
                    fontWeight: "bold",
                  }}
                >
                  Manage Admins
                </NavLink>

                <NavLink
                  id="adminSponsors"
                  className="nav-item nav-link"
                  to={`/${cluster.name}/admin/sponsors`}
                  activeStyle={{
                    fontWeight: "bold",
                  }}
                >
                  Manage Sponsors
                </NavLink>
              </ShowFor>
            </nav>
          </div>
        </div>
      )}
    </div>
  );
};
