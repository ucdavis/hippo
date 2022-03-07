import { useState, useContext } from "react";
import { NavLink } from "react-router-dom";
import AppContext from "./Shared/AppContext";
import HippoLogo from "./Shared/hippoLogo";

export const AppNav = () => {
  const [isOpen, setIsOpen] = useState(false);
  const toggle = () => setIsOpen(!isOpen);
  const [context] = useContext(AppContext);

  return (
    <div>
      <div className="row appheader justify-content-center">
        <div className="col-md-6 hippo">
          <HippoLogo />

          <h1>
            <img src="/media/ucdavis.svg" />
            HiPPO
          </h1>
          <p className="lede">High Performance Personnel Onboarding</p>
        </div>
      </div>
      <div className="row justify-content-center">
        <div className="col-md-6">
          <nav className="simple-nav">
            {context.account.canSponsor && (
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
            )}
            {context.user.detail.isAdmin && (
              <NavLink
                id="adminApprovals"
                className="nav-item nav-link"
                to="/admin/accountApprovals"
                activeStyle={{
                  fontWeight: "bold",
                }}
              >
                Manage Accounts
              </NavLink>
            )}
            {context.user.detail.isAdmin && (
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
            )}
            {context.user.detail.isAdmin && (
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
            )}
          </nav>
        </div>
      </div>
    </div>
  );
};
