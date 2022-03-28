import { useContext } from "react";
import AppContext from "../../Shared/AppContext";

export const AccountInfo = () => {
  const [context] = useContext(AppContext);

  return (
    <div className="row justify-content-center">
      <div className="col-md-8">
        <p>
          Welcome {context.user.detail.firstName} you already have an account.
          Enjoy!
        </p>
        <p>
          Documentation about this cluster can be found{" "}
          <a
            href="https://wiki.cse.ucdavis.edu/support/systems/farm"
            target={"_blank"}
            rel="noreferrer"
          >
            at the UC Davis Farm Wiki
          </a>
          .
        </p>
      </div>
    </div>
  );
};
