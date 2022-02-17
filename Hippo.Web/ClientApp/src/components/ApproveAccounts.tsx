import { useEffect, useState } from "react";

import { Account } from "../types";

import { authenticatedFetch } from "../util/api";

export const ApproveAccounts = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each account

  const [accounts, setAccounts] = useState<Account[]>();
  const [accountSubmitting, setAccountSubmitting] = useState<number>();

  useEffect(() => {
    const fetchAccounts = async () => {
      const response = await authenticatedFetch("/api/account/pending");

      if (response.ok) {
        setAccounts(await response.json());
      }
    };

    fetchAccounts();
  }, []);

  const handleApprove = async (account: Account) => {
    setAccountSubmitting(account.id);

    const response = await authenticatedFetch(
      `/api/account/approve/${account.id}`,
      { method: "POST" }
    );

    if (response.ok) {
      setAccountSubmitting(undefined);

      // remove the account from the list
      setAccounts(accounts?.filter((a) => a.id !== account.id));
    }
  };

  if (accounts === undefined) {
    return <div>Loading...</div>;
  } else {
    return (
      <div className="row justify-content-center">
        <div className="col-md-6">
          <p>There are {accounts.length} account(s) awaiting your approval</p>
          <table className="table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Submitted</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {accounts.map((account) => (
                <tr key={account.id}>
                  <td>{account.name}</td>
                  <td>{new Date(account.createdOn).toLocaleDateString()}</td>
                  <td>
                    <button
                      disabled={accountSubmitting !== undefined}
                      onClick={() => handleApprove(account)}
                      className="btn btn-primary"
                    >
                      {accountSubmitting === account.id
                        ? "Approving..."
                        : "Approve"}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    );
  }
};
