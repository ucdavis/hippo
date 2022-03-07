import { useEffect, useState } from "react";
import { Account } from "../types";
import { authenticatedFetch } from "../util/api";

export const SponsoredAccounts = () => {
  const [accounts, setAccounts] = useState<Account[]>();

  useEffect(() => {
    const fetchAccounts = async () => {
      const response = await authenticatedFetch("/api/account/sponsored");

      if (response.ok) {
        setAccounts(await response.json());
      }
    };

    fetchAccounts();
  }, []);

  if (accounts === undefined) {
    return <div>Loading...</div>;
  } else {
    return (
      <div className="row justify-content-center">
        <div className="col-md-6">
          <p>You have sponsored {accounts.length} account(s) </p>
          <table className="table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Approved On</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {accounts.map((account) => (
                <tr key={account.id}>
                  <td>{account.name}</td>
                  <td>{new Date(account.updatedOn).toLocaleDateString()}</td>
                  <td>{account.status}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    );
  }
};
