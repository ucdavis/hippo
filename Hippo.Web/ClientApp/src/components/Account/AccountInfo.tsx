import { useContext } from "react";
import AppContext from "../../Shared/AppContext";

export const AccountInfo = () => {
  const [context] = useContext(AppContext);

  return (
    <div className="row justify-content-center">
      <div className="col-md-8">
        <p>
          Welcome {context.user.detail.firstName} you already have an account,
          enjoy!
        </p>
      </div>
    </div>
  );
};
