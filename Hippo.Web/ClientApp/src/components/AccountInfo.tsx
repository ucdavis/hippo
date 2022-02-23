import { useContext } from "react";
import { Link } from "react-router-dom";
import AppContext from "../Shared/AppContext";

export const AccountInfo = () => {
  const [{ account }] = useContext(AppContext);
  const [context, setContext] = useContext(AppContext);

  return (
    <div className="row justify-content-center">
      <div className="col-md-6">
        <p>
          Welcome {context.user.detail.firstName} you already have an account,
          enjoy farm
        </p>
        {account.canSponsor && (
          <Link to="/approve">Click here to view any pending approvals</Link>
        )}
      </div>
    </div>
  );
};
