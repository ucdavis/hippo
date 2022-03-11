import { useEffect, useState } from "react";
import { Account, CreateSponsorPostModel } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";

export const Sponsors = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each account
  const [notification, setNotification] = usePromiseNotification();
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

    const req = authenticatedFetch(`/api/admin/RemoveSponsor/${account.id}`, {
      method: "POST",
    });

    setNotification(req, "Removing", "Sponsor Removed");

    const response = await req;
    if (response.ok) {
      setAdminRemoving(undefined);

      // remove the user from the list
      setAccounts(accounts?.filter((a) => a.id !== account.id));
    }
    //todo deal with error
  };

  const handleSubmit = async () => {
    const req = authenticatedFetch("/api/admin/createSponsor", {
      method: "POST",
      body: JSON.stringify(request),
    });

    setNotification(
      req,
      "Saving",
      async () => {
        const response = await req;
        let message = "";
        if (response.ok) {
          const newAccount = await response.json();
          let updatedAccounts = [];

          //check if the newAccount is already in the list
          if (accounts?.find((a) => a.id === newAccount.id)) {
            //if it is, update the account
            updatedAccounts = accounts
              ? accounts.map((a) => (a.id === newAccount.id ? newAccount : a))
              : [newAccount];
            message = "Sponsor Updated";
          } else {
            //if it is not, add it to the list and sort it
            updatedAccounts = accounts
              ? [...accounts, newAccount]
              : [newAccount];
            message = "Sponsor Added";
          }
          //sort the list
          setAccounts(
            updatedAccounts.sort((a, b) => a.name.localeCompare(b.name))
          );

          setRequest((r) => ({ ...r, lookup: "" }));
          setRequest((r) => ({ ...r, name: "" }));
        }
        return message;
      },
      async () => {
        const response = await req;
        if (response.status === 400) {
          const errorText = await response.text(); //Bad Request Text
          console.error(errorText);
          return errorText;
        } else {
          // const errorText = await response.text(); //This can contain exception info
          return "An error happened, please try again.";
        }
      }
    );
  };

  if (accounts === undefined) {
    return (
      <div className="row justify-content-center">
        <div className="col-md-6">Loading...</div>
      </div>
    );
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
