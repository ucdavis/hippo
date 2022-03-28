import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { Account, IRouteParams } from "../../types";
import { authenticatedFetch } from "../../util/api";

export const SponsoredAccounts = () => {
  const [accounts, setAccounts] = useState<Account[]>();

  const { cluster } = useParams<IRouteParams>();

  useEffect(() => {
    const fetchAccounts = async () => {
      const response = await authenticatedFetch(`/api/${cluster}/account/sponsored`);

      if (response.ok) {
        setAccounts(await response.json());
      }
    };

    fetchAccounts();
  }, [cluster]);

  if (accounts === undefined) {
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">Loading...</div>
      </div>
    );
  } else {
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">
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
