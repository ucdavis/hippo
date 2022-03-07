import { useContext } from "react";
import AppContext from "../Shared/AppContext";

export const PendingApproval = () => {
  const [context] = useContext(AppContext);

  return (
    <div className="row justify-content-center">
      <div className="col-md-6">
        <p>
          Welcome {context.user.detail.firstName} your account is pending
          approval. Please wait...
        </p>
      </div>
    </div>
  );
};
