import { useContext } from "react";
import { Redirect } from "react-router-dom";
import AppContext from "../Shared/AppContext";

// redirect to the proper page depending on current account status
export const Home = () => {
  const [{ accounts }] = useContext(AppContext);

  if (accounts.length === 0) {
    // no accounts, show request form
    return <Redirect to="/create" />;
  } else if (accounts.length === 1) {
    // one account, show page depending on status
    return (
      <Redirect
        to={`/${accounts[0].cluster}/${accounts[0].status.toLocaleLowerCase()}`}
      />
    );
  } else {
    return <Redirect to="/multiple" />;
  }
};
