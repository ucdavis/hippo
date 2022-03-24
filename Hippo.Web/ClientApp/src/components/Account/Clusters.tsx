import { useContext } from "react";
import { NavLink } from "react-router-dom";
import AppContext from "../../Shared/AppContext";

export const Clusters = () => {
  const [{ clusters }] = useContext(AppContext);

  return (
    <div className="row justify-content-center">
      <div className="col-md-8">
        <p>
          HiPPO supports multiple clusters. Please select a cluster to view.
        </p>
        <ul>
          {clusters.map((cluster) => (
            <li key={cluster.name}>
              <NavLink
                to={`/${cluster.name}`}
                >{cluster.description}</NavLink>
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
};
