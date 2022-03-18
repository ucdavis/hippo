import { useContext } from "react";
import AppContext from "../../Shared/AppContext";

export const PendingApproval = () => {
  const [context] = useContext(AppContext);

  return (
    <div className="row justify-content-center">
      <div className="col-md-8">
        <p>
          Welcome {context.user.detail.firstName} your account is pending
          approval. Your sponsor has been emailed and you will be notified when
          it has been acted on. Please check back later.
        </p>
      </div>
    </div>
  );
};
