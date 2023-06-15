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
        <p>
          HiPPO supports multiple clusters. Please select a cluster to view.
        </p>
        <ul className="list-clusters">
          {clusters.map((cluster) => (
            <li key={cluster.name}>
              <NavLink 
                to={`/${cluster.name}`}
                style={{
                  fontWeight: "bolder"
                }}
              >
                {`${cluster.name}:`}
              </NavLink>
              <p>{cluster.description}</p>
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
};
