import { useContext } from "react";
import { NavLink, useMatch } from "react-router-dom";
import AppContext from "./Shared/AppContext";
import HippoLogo from "./Shared/hippoLogo";
import { ShowFor } from "./Shared/ShowFor";

export const AppNav = () => {
  const [{ clusters, accounts, lastPuppetSync }] = useContext(AppContext);
  const match = useMatch("/:cluster/*");
  const cluster = clusters.find((c) => c.name === match?.params.cluster);

  const accountInCluster =
    cluster && accounts.some((a) => a.cluster === cluster.name);

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
              <NavLink to="/clusters">Change Cluster</NavLink>
            </p>
          )}
          {lastPuppetSync && (
            <p className="sync-status">
              Last synced with Puppet{" "}
              {new Date(lastPuppetSync).toLocaleDateString("en-us", {
                weekday: "long",
                month: "short",
                day: "numeric",
                hour: "numeric",
                minute: "numeric",
              })}
            </p>
          )}
        </div>
      </div>
      {/* Only show links if a cluster has been identified */}
      {cluster && (
        <div className="row justify-content-center">
          <div className="col-md-8">
            <nav className="simple-nav">
              {accountInCluster && (
                <NavLink
                  id="myAccount"
                  to={`/${cluster.name}/myaccount`}
                  className="nav-item nav-link"
                  style={({ isActive }) =>
                    isActive ? { fontWeight: "bold" } : {}
                  }
                >
                  My Account
                </NavLink>
              )}
              <ShowFor roles={["GroupAdmin"]}>
                <NavLink
                  id="sponsorApprove"
                  to={`/${cluster.name}/approve`}
                  className="nav-item nav-link"
                  style={({ isActive }) =>
                    isActive ? { fontWeight: "bold" } : {}
                  }
                >
                  Pending Approvals
                </NavLink>
                <NavLink
                  id="activeAccounts"
                  to={`/${cluster.name}/activeaccounts`}
                  className="nav-item nav-link"
                  style={({ isActive }) =>
                    isActive ? { fontWeight: "bold" } : {}
                  }
                >
                  Active Accounts
                </NavLink>
              </ShowFor>
              <ShowFor roles={["ClusterAdmin"]}>
                <NavLink
                  id="groups"
                  className="nav-item nav-link"
                  to={`/${cluster.name}/admin/groups`}
                  style={({ isActive }) =>
                    isActive ? { fontWeight: "bold" } : {}
                  }
                >
                  Groups
                </NavLink>

                <NavLink
                  id="clusterAdmins"
                  className="nav-item nav-link"
                  to={`/${cluster.name}/admin/clusteradmins`}
                  style={({ isActive }) =>
                    isActive ? { fontWeight: "bold" } : {}
                  }
                >
                  Cluster Admins
                </NavLink>
              </ShowFor>
            </nav>
          </div>
        </div>
      )}
    </div>
  );
};
