import { NavLink } from "react-router-dom";
import HippoLogo from "./Shared/hippoLogo";
import { ShowFor } from "./Shared/ShowFor";

export const AppNav = () => {
  return (
    <div>
      <div className="row appheader justify-content-center">
        <div className="col-md-8 hippo">
          <HippoLogo />

          <h1>
            <img src="/media/ucdavis.svg" alt="UC DAVIS" />
            HiPPO
          </h1>
          <p className="lede">High Performance Personnel Onboarding</p>
        </div>
      </div>
      <div className="row justify-content-center">
        <div className="col-md-8">
          <nav className="simple-nav">
            <ShowFor roles={["Sponsor"]}>
              <NavLink
                id="sponsorApprove"
                to="/approve"
                className="nav-item nav-link"
                activeStyle={{
                  fontWeight: "bold",
                }}
              >
                Pending Approvals
              </NavLink>
              <NavLink
                id="sponsored"
                to="/sponsored"
                className="nav-item nav-link"
                activeStyle={{
                  fontWeight: "bold",
                }}
              >
                Sponsored Accounts
              </NavLink>
            </ShowFor>
            <ShowFor roles={["Admin"]}>
              <NavLink
                id="adminApprovals"
                className="nav-item nav-link"
                to="/admin/accountApprovals"
                activeStyle={{
                  fontWeight: "bold",
                }}
              >
                Pending Accounts
              </NavLink>

              <NavLink
                id="AdminIndex"
                className="nav-item nav-link"
                to="/admin/users"
                activeStyle={{
                  fontWeight: "bold",
                }}
              >
                Manage Admins
              </NavLink>

              <NavLink
                id="adminSponsors"
                className="nav-item nav-link"
                to="/admin/sponsors"
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
    </div>
  );
};
