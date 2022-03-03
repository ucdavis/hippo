import { useState, useContext } from "react";
import { Link } from "react-router-dom";
import AppContext from "./Shared/AppContext";
import {
  Collapse,
  DropdownItem,
  DropdownMenu,
  DropdownToggle,
  Nav,
  Navbar,
  NavbarBrand,
  NavbarText,
  NavbarToggler,
  NavItem,
  NavLink,
  UncontrolledDropdown,
} from "reactstrap";
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
              <Link to="/approve" className="nav-item nav-link active">
                Pending Approvals
              </Link>
            )}
            {context.user.detail.isAdmin && (
              <Link className="nav-item nav-link active" to="/admin/approvals">
                Manage Accounts
              </Link>
            )}
            {context.user.detail.isAdmin && (
              <Link className="nav-item nav-link" to="/admin/index">
                Manage Admins
              </Link>
            )}
            {context.user.detail.isAdmin && (
              <Link className="nav-item nav-link" to="/admin/sponsors">
                Manage Sponsors
              </Link>
            )}
          </nav>
        </div>
      </div>
    </div>
  );
};
