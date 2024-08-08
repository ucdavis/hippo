import { useContext } from "react";
import { NavLink } from "react-router-dom";
import AppContext from "../../Shared/AppContext";
import { ShowFor } from "../../Shared/ShowFor";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipTitle from "../../Shared/Layout/HipTitle";
import HipBody from "../../Shared/Layout/HipBody";

export const Clusters = () => {
  const [{ clusters }] = useContext(AppContext);

  return (
    <HipMainWrapper>
      <nav className="simple-nav">
        <ShowFor roles={["System"]}>
          <NavLink
            id="adminClusters"
            className="nav-item nav-link"
            to={`/clusteradmin/clusters`}
            style={({ isActive }) => (isActive ? { fontWeight: "bold" } : {})}
          >
            Manage Clusters
          </NavLink>
        </ShowFor>
      </nav>
      <HipTitle title="Clusters at Hippo" />
      <p>HiPPO supports multiple clusters. Please select a cluster to view.</p>
      <hr />
      <HipBody>
        <ul className="list-clusters">
          {clusters.map((cluster) => (
            <li key={cluster.name}>
              <h4>
                <NavLink
                  to={`/${cluster.name}`}
                  style={{
                    fontWeight: "bolder",
                  }}
                >
                  {`${cluster.name}:`}
                </NavLink>
              </h4>

              <p>
                {cluster.description}
                {cluster.email && (
                  <>
                    <br />
                    Contact:{" "}
                    <a
                      href={`mailto:${cluster.email}?subject=Inquiry about cluster ${cluster.name}`}
                    >
                      {cluster.email}
                    </a>
                  </>
                )}
              </p>
            </li>
          ))}
        </ul>
      </HipBody>
    </HipMainWrapper>
  );
};
