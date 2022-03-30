import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { Account, CreateSponsorPostModel, IRouteParams } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { TransferSponsor } from "./TransferSponsor";

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
  const { cluster } = useParams<IRouteParams>();

  const [getConfirmation] = useConfirmationDialog<string>(
    {
      title: "Remove Sponsor",
      message: "Are you sure you want to remove this sponsor?",
    },
    []
  );

  useEffect(() => {
    const fetchSponsors = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/admin/sponsors`
      );

      if (response.ok) {
        setAccounts(await response.json());
      } else {
        alert("Error");
      }
    };

    fetchSponsors();
  }, [cluster]);

  const handleRemove = async (account: Account) => {
    const [confirmed] = await getConfirmation();
    if (!confirmed) {
      return;
    }

    setAdminRemoving(account.id);

    const req = authenticatedFetch(
      `/api/${cluster}/admin/RemoveSponsor/${account.id}`,
      {
        method: "POST",
      }
    );

    setNotification(req, "Removing", "Sponsor Removed");

    const response = await req;
    if (response.ok) {
      setAdminRemoving(undefined);

      // remove the user from the list
      setAccounts(accounts?.filter((a) => a.id !== account.id));
    }
    //todo deal with error
  };

  const handleTransfer = async (oldAccount: Account, newAccount: Account) => {
    // filter out old account and push new account at the front
    setAccounts((accts) =>
      accts
        ? [newAccount, ...accts.filter((a) => a.id !== oldAccount.id && a.id !== newAccount.id)]
        : [newAccount]
    );
  };

  const handleSubmit = async () => {
    const req = authenticatedFetch(`/api/${cluster}/admin/createSponsor`, {
      method: "POST",
      body: JSON.stringify(request),
    });

    setNotification(
      req,
      "Saving",
      (r) => {
        // HTTP 201 is "created"
        if (r.status === 201) {
          return "Sponsor Created";
        } else {
          return "Sponsor Updated";
        }
      },
      async (r) => {
        if (r.status === 400) {
          const errorText = await response.text(); //Bad Request Text
          return errorText;
        } else {
          return "An error happened, please try again.";
        }
      }
    );

    const response = await req;

    if (response.ok) {
      const newAccount = await response.json();
      let updatedAccounts = [];
      //check if the newAccount is already in the list
      if (accounts?.find((a) => a.id === newAccount.id)) {
        //if it is, update the account
        updatedAccounts = accounts
          ? accounts.map((a) => (a.id === newAccount.id ? newAccount : a))
          : [newAccount];
      } else {
        //if it is not, add it to the list and sort it
        updatedAccounts = accounts ? [...accounts, newAccount] : [newAccount];
      }
      //sort the list
      setAccounts(updatedAccounts.sort((a, b) => a.name.localeCompare(b.name)));
      setRequest((r) => ({ ...r, lookup: "", name: "" }));
    }
  };

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
          <br />
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
          <button
            disabled={notification.pending}
            className="btn btn-primary"
            onClick={handleSubmit}
          >
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
                    <TransferSponsor
                      account={account}
                      transferSponsor={handleTransfer}
                      transferUrl={`/api/${cluster}/admin/changeSponsorOwner/`}
                      disabled={notification.pending}
                    ></TransferSponsor>
                    {" | "}
                    <button
                      disabled={notification.pending}
                      onClick={() => handleRemove(account)}
                      className="btn btn-danger"
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
