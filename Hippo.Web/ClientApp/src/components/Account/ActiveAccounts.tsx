import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { AccountModel, IRouteParams } from "../../types";
import { authenticatedFetch } from "../../util/api";

export const ActiveAccounts = () => {
  const [accounts, setAccounts] = useState<AccountModel[]>();

  const { cluster } = useParams<IRouteParams>();

  useEffect(() => {
    const fetchAccounts = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/account/active`
      );

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
    const groupCount = new Set(
      accounts
        .map((a) => a.groups)
        .flat()
        .filter((g) => g !== null)
    ).size;
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">
          <p>
            You have {accounts.length} active account(s) in {groupCount}{" "}
            group(s)
          </p>
          <table className="table">
            <thead>
              <tr>
                <th>Groups</th>
                <th>Name</th>
                <th>Approved On</th>
              </tr>
            </thead>
            <tbody>
              {accounts.map((account) => (
                <tr key={account.id}>
                  <td>{account.groups.join(", ")}</td>
                  <td>{account.name}</td>
                  <td>{new Date(account.updatedOn).toLocaleDateString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    );
  }
};
