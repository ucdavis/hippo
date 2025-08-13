import { useContext, useState } from "react";
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

export const AppNav = () => {
  const [{ clusters, accounts, lastPuppetSync }] = useContext(AppContext);
  const match = useMatch("/:cluster/*");
  const cluster = clusters.find((c) => c.name === match?.params.cluster);

  const accountInCluster =
    cluster && accounts.some((a) => a.cluster === cluster.name);

  const [dropdownOpenAdmin, setDropdownOpen] = useState(false);
  const [dropdownOpenOrders, setDropdownOpenOrders] = useState(false);
  const [dropdownOpenReports, setDropdownOpenReports] = useState(false);

  const toggleAdmin = () => setDropdownOpen((prevState) => !prevState);
  const toggleOrders = () => setDropdownOpenOrders((prevState) => !prevState);
  const toggleReports = () => setDropdownOpenReports((prevState) => !prevState);

  return (
    <div>
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
              <ShowFor roles={["GroupAdmin", "ClusterAdmin", "System"]}>
                <Dropdown
                  isOpen={dropdownOpenAdmin}
                  toggle={toggleAdmin}
                  id="adminNav"
                >
                  <DropdownToggle className="nav-item nav-link" caret>
                    Admin
                  </DropdownToggle>
                  <DropdownMenu>
                    <ShowFor roles={["GroupAdmin"]}>
                      <DropdownItem>
                        <NavLink
                          id="sponsorApprove"
                          to={`/${cluster.name}/approve`}
                          className="nav-dropdown-item nav-link"
                          style={({ isActive }) =>
                            isActive ? { fontWeight: "bold" } : {}
                          }
                        >
                          Pending Approvals
                        </NavLink>
                      </DropdownItem>
                    </ShowFor>
                    <ShowFor roles={["GroupAdmin"]}>
                      <DropdownItem>
                        <NavLink
                          id="activeAccounts"
                          to={`/${cluster.name}/activeaccounts`}
                          className="nav-dropdown-item nav-link"
                          style={({ isActive }) =>
                            isActive ? { fontWeight: "bold" } : {}
                          }
                        >
                          Active Accounts
                        </NavLink>
                      </DropdownItem>
                    </ShowFor>
                    <ShowFor roles={["ClusterAdmin"]}>
                      <DropdownItem>
                        <NavLink
                          id="groups"
                          className="nav-dropdown-item nav-link"
                          to={`/${cluster.name}/admin/groups`}
                          style={({ isActive }) =>
                            isActive ? { fontWeight: "bold" } : {}
                          }
                        >
                          Groups
                        </NavLink>
                      </DropdownItem>
                      <DropdownItem>
                        <NavLink
                          id="clusterAdmins"
                          className="nav-dropdown-item nav-link"
                          to={`/${cluster.name}/admin/clusteradmins`}
                          style={({ isActive }) =>
                            isActive ? { fontWeight: "bold" } : {}
                          }
                        >
                          Cluster Admins
                        </NavLink>
                      </DropdownItem>
                      <DropdownItem>
                        <NavLink
                          id="financialAdmins"
                          className="nav-dropdown-item nav-link"
                          to={`/${cluster.name}/admin/financialadmins`}
                          style={({ isActive }) =>
                            isActive ? { fontWeight: "bold" } : {}
                          }
                        >
                          Financial Admins
                        </NavLink>
                      </DropdownItem>
                      <DropdownItem>
                        <a
                          href="http://caeshelp.ucdavis.edu/?appname=Hippo"
                          target="_blank"
                          rel="noopener noreferrer"
                          className="nav-dropdown-item nav-link"
                        >
                          Hippo Technical Support
                        </a>
                      </DropdownItem>
                    </ShowFor>
                  </DropdownMenu>
                </Dropdown>
              </ShowFor>
              <ShowFor
                roles={["System", "FinancialAdmin"]}
                condition={!cluster.allowOrders}
              >
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
                <Dropdown
                  isOpen={dropdownOpenOrders}
                  toggle={toggleOrders}
                  id="ordersNav"
                >
                  <DropdownToggle className="nav-item nav-link" caret>
                    Orders
                  </DropdownToggle>
                  <DropdownMenu>
                    <ShowFor roles={["System", "FinancialAdmin"]}>
                      <DropdownItem>
                        <NavLink
                          id="financialDetails"
                          className="nav-dropdown-item nav-link"
                          to={`/${cluster.name}/financial/financialdetails`}
                          style={({ isActive }) =>
                            isActive ? { fontWeight: "bold" } : {}
                          }
                        >
                          Financial
                        </NavLink>
                      </DropdownItem>
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
                      <DropdownItem>
                        <NavLink
                          id="products"
                          to={`/${cluster.name}/product/index`}
                          className="nav-dropdown-item nav-link"
                          style={({ isActive }) =>
                            isActive ? { fontWeight: "bold" } : {}
                          }
                        >
                          Products
                        </NavLink>
                      </DropdownItem>
                      <DropdownItem>
                        <NavLink
                          id="myorders"
                          to={`/${cluster.name}/order/myorders`}
                          className="nav-dropdown-item nav-link"
                          style={({ isActive }) =>
                            isActive ? { fontWeight: "bold" } : {}
                          }
                        >
                          My Orders
                        </NavLink>
                      </DropdownItem>
                    </ShowFor>
                    <ShowFor
                      roles={["ClusterAdmin", "System", "FinancialAdmin"]}
                      condition={cluster.allowOrders}
                    >
                      <DropdownItem>
                        <NavLink
                          id="adminorders"
                          to={`/${cluster.name}/order/adminorders`}
                          className="nav-dropdown-item nav-link"
                          style={({ isActive }) =>
                            isActive ? { fontWeight: "bold" } : {}
                          }
                        >
                          Admin Orders
                        </NavLink>
                      </DropdownItem>
                    </ShowFor>
                  </DropdownMenu>
                </Dropdown>
              </ShowFor>

              <ShowFor roles={["System", "ClusterAdmin", "FinancialAdmin"]}>
                <Dropdown
                  isOpen={dropdownOpenReports}
                  toggle={toggleReports}
                  id="reportsNav"
                >
                  <DropdownToggle className="nav-item nav-link" caret>
                    Reports
                  </DropdownToggle>
                  <DropdownMenu>
                    <ShowFor
                      roles={["System", "FinancialAdmin", "ClusterAdmin"]}
                    >
                      <DropdownItem>
                        <NavLink
                          id="paymentsReport"
                          to={`/${cluster.name}/report/payments`}
                          className="nav-dropdown-item nav-link"
                          style={({ isActive }) =>
                            isActive ? { fontWeight: "bold" } : {}
                          }
                        >
                          Payments
                        </NavLink>
                      </DropdownItem>
                    </ShowFor>
                    <ShowFor
                      roles={["System", "ClusterAdmin", "FinancialAdmin"]}
                    >
                      <DropdownItem>
                        <NavLink
                          id="expiringOrdersReport"
                          to={`/${cluster.name}/report/order/ExpiringOrders`}
                          className="nav-dropdown-item nav-link"
                          style={({ isActive }) =>
                            isActive ? { fontWeight: "bold" } : {}
                          }
                        >
                          Expiring Orders
                        </NavLink>
                      </DropdownItem>
                    </ShowFor>
                    <ShowFor
                      roles={["System", "ClusterAdmin", "FinancialAdmin"]}
                    >
                      <DropdownItem>
                        <NavLink
                          id="problemOrdersReport"
                          to={`/${cluster.name}/report/order/ProblemOrders`}
                          className="nav-dropdown-item nav-link"
                          style={({ isActive }) =>
                            isActive ? { fontWeight: "bold" } : {}
                          }
                        >
                          Problem Orders
                        </NavLink>
                      </DropdownItem>
                    </ShowFor>
                    <ShowFor roles={["System", "ClusterAdmin"]}>
                      <DropdownItem>
                        <NavLink
                          id="archivedOrdersReport"
                          to={`/${cluster.name}/report/order/ArchivedOrders`}
                          className="nav-dropdown-item nav-link"
                          style={({ isActive }) =>
                            isActive ? { fontWeight: "bold" } : {}
                          }
                        >
                          Archived Orders
                        </NavLink>
                      </DropdownItem>
                    </ShowFor>
                    <ShowFor roles={["System", "ClusterAdmin"]}>
                      <DropdownItem>
                        <NavLink
                          id="accountDeactivationsReport"
                          to={`/${cluster.name}/report/accountdeactivations`}
                          className="nav-dropdown-item nav-link"
                          style={({ isActive }) =>
                            isActive ? { fontWeight: "bold" } : {}
                          }
                        >
                          Account Deactivations
                        </NavLink>
                      </DropdownItem>
                    </ShowFor>
                  </DropdownMenu>
                </Dropdown>
              </ShowFor>
            </nav>
          </div>
        </div>
      )}
    </div>
  );
};
