import { useState } from "react";
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

  return (
    <div className="row appheader justify-content-center">
      <div className="col-md-6">
        <HippoLogo />

        <h1>
          <img src="/media/ucdavis.svg" />
          HiPPO
        </h1>
        <p className="lede">High Performance Personnel Onboarding</p>
      </div>
    </div>
  );
};
