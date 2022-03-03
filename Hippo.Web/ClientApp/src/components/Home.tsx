import { useContext } from "react";
import { Redirect } from "react-router-dom";
import AppContext from "../Shared/AppContext";

// redirect to the proper page depending on current account status
export const Home = () => {
  const [{ account, user }] = useContext(AppContext);
  if (user?.detail?.isAdmin) {
    return <Redirect to="/admin/index" />;
  }

  return <Redirect to={`/${account.status.toLocaleLowerCase()}`} />;
};
