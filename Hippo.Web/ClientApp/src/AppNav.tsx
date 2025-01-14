import { useContext } from "react";
import { NavLink, useMatch } from "react-router-dom";
import AppContext from "./Shared/AppContext";
import HippoLogo from "./Shared/hippoLogo";
import { ShowFor } from "./Shared/ShowFor";

import {
  Dropdown,
  DropdownToggle,
  DropdownMenu,
  DropdownItem,
} from "reactstrap";
import PropTypes from "prop-types";

export const AppNav = () => {
  const [{ clusters, accounts, lastPuppetSync }] = useContext(AppContext);
  const match = useMatch("/:cluster/*");
  const cluster = clusters.find((c) => c.name === match?.params.cluster);

  const accountInCluster =
    cluster && accounts.some((a) => a.cluster === cluster.name);

  const [dropdownOpen, setDropdownOpen] = useState(false);

  const toggle = () => setDropdownOpen((prevState) => !prevState);

  return (
    <div>
      <div className="d-flex p-5">
      <Dropdown isOpen={dropdownOpen} toggle={toggle} direction={direction}>
        <DropdownToggle caret>Dropdown</DropdownToggle>
        <DropdownMenu {...args}>
          <DropdownItem header>Header</DropdownItem>
          <DropdownItem>Some Action</DropdownItem>
          <DropdownItem text>Dropdown Item Text</DropdownItem>
          <DropdownItem disabled>Action (disabled)</DropdownItem>
          <DropdownItem divider />
          <DropdownItem>Foo Action</DropdownItem>
          <DropdownItem>Bar Action</DropdownItem>
          <DropdownItem>Quo Action</DropdownItem>
        </DropdownMenu>
      </Dropdown>
      <div className="row appheader justify-content-center">
        <div className="col-md-12 hippo">
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
          <div className="col-md-12">
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
              <ShowFor roles={["System", "FinancialAdmin"]}>
                <NavLink
                  id="financialDetails"
                  className="nav-item nav-link"
                  to={`/${cluster.name}/financial/financialdetails`}
                  style={({ isActive }) =>
                    isActive ? { fontWeight: "bold" } : {}
                  }
                >
                  Financial
                </NavLink>
              </ShowFor>
              <ShowFor
                roles={[
                  "System",
                  "ClusterAdmin",
                  "GroupAdmin",
                  "FinancialAdmin",
                ]}
                condition={cluster.allowOrders}
              >
                <NavLink
                  id="products"
                  to={`/${cluster.name}/product/index`}
                  className="nav-item nav-link"
                  style={({ isActive }) =>
                    isActive ? { fontWeight: "bold" } : {}
                  }
                >
                  Products
                </NavLink>
                <NavLink
                  id="myorders"
                  to={`/${cluster.name}/order/myorders`}
                  className="nav-item nav-link"
                  style={({ isActive }) =>
                    isActive ? { fontWeight: "bold" } : {}
                  }
                >
                  My Orders
                </NavLink>
              </ShowFor>
              <ShowFor
                roles={["ClusterAdmin", "System", "FinancialAdmin"]}
                condition={cluster.allowOrders}
              >
                <NavLink
                  id="adminorders"
                  to={`/${cluster.name}/order/adminorders`}
                  className="nav-item nav-link"
                  style={({ isActive }) =>
                    isActive ? { fontWeight: "bold" } : {}
                  }
                >
                  Admin Orders
                </NavLink>
              </ShowFor>
              {/* TODO: make a index page to choose the reports when we have more? */}
              <ShowFor
                roles={["ClusterAdmin", "System", "FinancialAdmin"]}
                condition={cluster.allowOrders}
              >
                <NavLink
                  id="payments"
                  to={`/${cluster.name}/report/payments`}
                  className="nav-item nav-link"
                  style={({ isActive }) =>
                    isActive ? { fontWeight: "bold" } : {}
                  }
                >
                  Reports
                </NavLink>
              </ShowFor>
            </nav>
          </div>
        </div>
      )}
    </div>
  );
};
