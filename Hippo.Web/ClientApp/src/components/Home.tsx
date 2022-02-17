import { useContext } from "react";
import { Redirect } from "react-router-dom";
import AppContext from "../Shared/AppContext";

// redirect to the proper page depending on current account status
export const Home = () => {
  const { account } = useContext(AppContext);

  return <Redirect to={`/${account.status.toLocaleLowerCase()}`} />;
};
