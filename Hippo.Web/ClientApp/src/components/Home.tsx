import { useEffect, useState } from "react";
import { Redirect } from "react-router-dom";
import { Account } from "../types";

import { authenticatedFetch } from "../util/api";

// query for account status and redirect to the proper page depending on the results
export const Home = () => {
  const [accountStatus, setAccountStatus] = useState<string>();

  useEffect(() => {
    // query for user account status
    const fetchAccount = async () => {
      const response = await authenticatedFetch("/api/account/get");

      if (response.ok) {
        if (response.status === 204) {
          // no content means we have no account record for this person
          setAccountStatus("create");
        } else {
          const account = (await response.json()) as Account;
          setAccountStatus(account.status.toLocaleLowerCase());
        }
      }

      // TODO: handle error case
    };

    fetchAccount();
  }, []);

  return (
    <div className="row justify-content-center">
      <div className="col-md-6">
        {accountStatus === undefined && <p>Loading...</p>}
        {accountStatus !== undefined && <Redirect to={`/${accountStatus}`} />}
      </div>
    </div>
  );
};
