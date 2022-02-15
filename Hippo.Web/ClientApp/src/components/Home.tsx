import { useEffect, useState } from "react";

import { Account } from "../types";
import { authenticatedFetch } from "../util/api";

import { RequestForm } from "./RequstForm";

export const Home = () => {
  const [account, setAccount] = useState<Account>();

  useEffect(() => {
    // query for user account status
    const fetchAccount = async () => {
      const response = await authenticatedFetch("/api/account/get");

      if (response.ok) {
        if (response.status === 204) {
          // no content means we have no account record for this person
          setAccount({ id: 0, status: "NonExistant" } as Account);
        } else {
          // else we have the account
          setAccount(await response.json());

          // TODO: we are hardcoding no account for now to test
          // setAccount({ id: 0, status: "NonExistant" } as Account);
        }
      }
    };

    fetchAccount();
  }, []);

  return (
    <div className="row justify-content-center">
      <div className="col-md-6">
        {!account && <p>Loading...</p>}
        {account && account.status === "NonExistant" && <RequestForm />}
        {account && account.status !== "NonExistant" && (
          <p>You have an account, TODO</p>
        )}
      </div>
    </div>
  );
};
