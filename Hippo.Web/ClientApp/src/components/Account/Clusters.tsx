import { useContext } from "react";
import { NavLink } from "react-router-dom";
import AppContext from "../../Shared/AppContext";
import { ShowFor } from "../../Shared/ShowFor";

export const Clusters = () => {
  const [{ clusters }] = useContext(AppContext);

  return (
    <div className="row justify-content-center">
      <div className="col-md-8">
        <nav className="simple-nav">
          <ShowFor roles={["System"]}>
            <NavLink
              id="adminClusters"
              className="nav-item nav-link"
              to={`/clusteradmin/clusters`}
              activeStyle={{
                fontWeight: "bold",
              }}
            >
              Manage Clusters
            </NavLink>
          </ShowFor>
        </nav>
        <h3>Clusters at Hippo</h3>
        <p>
          HiPPO supports multiple clusters. Please select a cluster to view.
        </p>
        <hr />

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

              {cluster.email && (
                <p>
                  {cluster.description}
                  <br />
                  Contact:{" "}
                  <a href={`mailto:${cluster.email}`}>{cluster.email}</a>
                </p>
              )}
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
};
