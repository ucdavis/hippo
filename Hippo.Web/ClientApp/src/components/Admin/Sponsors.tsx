import { useEffect, useState } from "react";

import { Account, CreateSponsorPostModel } from "../../types";

import { authenticatedFetch } from "../../util/api";

export const Sponsors = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each account

  const [accounts, setAccounts] = useState<Account[]>();
  const [adminRemoving, setAdminRemoving] = useState<number>();
  const [request, setRequest] = useState<CreateSponsorPostModel>({
    lookup: "",
    name: "",
  });

  useEffect(() => {
    const fetchSponsors = async () => {
      const response = await authenticatedFetch("/api/admin/sponsors");

      if (response.ok) {
        setAccounts(await response.json());
      } else {
        alert("Error");
      }
    };

    fetchSponsors();
  }, []);

  const handleRemove = async (account: Account) => {
    setAdminRemoving(account.id);

    const response = await authenticatedFetch(
      `/api/admin/RemoveSponsor/${account.id}`,
      {
        method: "POST",
      }
    );

    if (response.ok) {
      setAdminRemoving(undefined);

      // remove the user from the list
      setAccounts(accounts?.filter((a) => a.id !== account.id));
    }
    //todo deal with error
  };

  const handleSubmit = async () => {
    const response = await authenticatedFetch("/api/admin/createSponsor", {
      method: "POST",
      body: JSON.stringify(request),
    });

    if (response.ok) {
      const newAccount = await response.json();
      //Add the user to the list
      setAccounts((r) => (r ? [...r, newAccount] : [newAccount]));
      setRequest((r) => ({ ...r, lookup: "" }));
      setRequest((r) => ({ ...r, name: "" }));
    } else {
      if (response.status === 400) {
        const errorText = await response.text(); //Bad Request Text
        console.error(errorText);
        alert(errorText);
      } else {
        // const errorText = await response.text(); //This can contain exception info
        alert("An error happened, please try again.");
      }
    }
  };

  if (accounts === undefined) {
    return <div>Loading...</div>;
  } else {
    return (
      <div className="row justify-content-center">
        <div className="col-md-6">
          <div className="form-group">
            <label className="form-label">Email or Kerberos</label>

            <input
              className="form-control"
              id="emailLookup"
              placeholder="example@ucdavis.edu"
              value={request.lookup}
              onChange={(e) =>
                setRequest((r) => ({ ...r, lookup: e.target.value }))
              }
            ></input>
          </div>
          <div className="form-group">
            <label className="form-label">Override Account Name</label>
            <input
              className="form-control"
              id="overrideName"
              placeholder="Override Name"
              value={request.name}
              onChange={(e) =>
                setRequest((r) => ({ ...r, name: e.target.value }))
              }
            ></input>
          </div>
          <br />
          <button className="btn btn-primary" onClick={handleSubmit}>
            Add Sponsor
          </button>
          <hr />

          <p>There are {accounts.length} sponsor accounts</p>
          <table className="table">
            <thead>
              <tr>
                <th>Account Name</th>
                <th>Sponsor</th>
                <th>Email</th>
              </tr>
            </thead>
            <tbody>
              {accounts.map((account) => (
                <tr key={account.id}>
                  <td>{account.name}</td>
                  <td>{account.owner?.name}</td>
                  <td>{account.owner?.email}</td>
                  <td>
                    <button
                      disabled={adminRemoving !== undefined}
                      onClick={() => handleRemove(account)}
                      className="btn btn-primary"
                    >
                      {adminRemoving === account.id ? "Removing..." : "Remove"}
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
