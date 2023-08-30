import { useContext } from "react";
import { useRouteMatch } from "react-router-dom";
import AppContext from "../../Shared/AppContext";
import { IRouteParams } from "../../types";

export const AccountInfo = () => {
  const [context] = useContext(AppContext);
  const match = useRouteMatch<IRouteParams>("/:cluster/:path");
  const groups =
    context.accounts.filter((a) => a.cluster === match?.params.cluster)[0]
      ?.groups ?? [];

  return (
    <div className="row justify-content-center">
      <div className="col-md-8 text-center">
        <p>
          Welcome {context.user.detail.firstName} you have an account{" "}
          {groups.length && "in group(s) " + groups.join(", ") + " "}
          on {match?.params.cluster}. Enjoy!
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
