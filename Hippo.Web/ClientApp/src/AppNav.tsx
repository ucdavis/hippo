import { useContext, useState } from "react";
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
import AppContext from "./Shared/AppContext";
import HippoLogo from "./Shared/hippoLogo";

export const AppNav = () => {
  const [isOpen, setIsOpen] = useState(false);
  const user = useContext(AppContext).user;
  const toggle = () => setIsOpen(!isOpen);

  return (
    <div className="row appheader justify-content-center">
      <div className="col-md-6">
        <HippoLogo />

        <h1>
          <img src="/media/ucdavis.svg" />
          HiPPO
        </h1>
        <p className="lede">High Performance Personnel Onboarding</p>
        <p className="login">
          {user.detail.name}
          <span className="discreet">
            {" "}
            <form action="/Account/Logout" method="post" id="logoutForm">
              <button className="btn btn-link btn-sm" type="submit">
                • log out
              </button>
            </form>
          </span>
        </p>
      </div>
    </div>
  );
};
