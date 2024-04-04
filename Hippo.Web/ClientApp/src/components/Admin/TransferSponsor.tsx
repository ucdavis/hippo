import React, { useState } from "react";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { AccountModel } from "../../types";
import { authenticatedFetch, parseBadRequest } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { notEmptyOrFalsey } from "../../util/ValueChecks";

interface Props {
  account: AccountModel;
  transferSponsor: (oldAccount: AccountModel, newAccount: AccountModel) => void;
  transferUrl: string;
  disabled?: boolean;
}

export const TransferSponsor = (props: Props) => {
  const [lookup, setLookup] = useState("");
  const [name, setName] = useState("");

  const [notification, setNotification] = usePromiseNotification();

  const [getConfirmation] = useConfirmationDialog<{
    lookup: string;
    name: string;
  }>(
    {
      title: "Transfer Sponsor",
      message: (setReturn) => (
        <div>
          <p>
            You are about to transfer {props.account.name} to another sponsor.
            Anyone sponsored by this sponsor will be transferred to the new
            sponsor and all account history will be retained.
          </p>
          <p>Please enter new sponsor's information below.</p>
          <div className="form-group">
            <label htmlFor="lookup">Email or Kerberos</label>
            <input
              className="form-control"
              id="lookup"
              required
              placeholder="example@ucdavis.edu"
              value={lookup}
              onChange={(e) => {
                setLookup(e.target.value);
                setReturn({ lookup: e.target.value, name });
              }}
            />
          </div>
          <div className="form-group">
            <label htmlFor="name">Account Name</label>
            <input
              className="form-control"
              id="name"
              required
              value={name}
              onChange={(e) => {
                setName(e.target.value);
                setReturn({ name: e.target.value, lookup });
              }}
            />
          </div>
        </div>
      ),
      canConfirm:
        notEmptyOrFalsey(lookup) &&
        notEmptyOrFalsey(name) &&
        !notification.pending,
    },
    [lookup, setLookup, name, setName, notification.pending],
  );

  const transfer = async () => {
    const [confirmed, transferData] = await getConfirmation();
    if (!confirmed) {
      return;
    }

    const request = authenticatedFetch(
      `${props.transferUrl}${props.account.id}`,
      {
        method: "POST",
        body: JSON.stringify(transferData),
      },
    );

    setNotification(
      request,
      "Saving",
      "Sponsor Transfer Successful",
      async (r) => {
        if (r.status === 400) {
          const errors = await parseBadRequest(response);
          return errors;
        } else {
          return "An error happened, please try again.";
        }
      },
    );

    const response = await request;

    if (response.ok) {
      const newAccount = (await response.json()) as AccountModel;
      props.transferSponsor(props.account, newAccount);
    }
  };
  return (
    <button
      disabled={props.disabled === true}
      onClick={transfer}
      className="btn btn-primary"
    >
      Transfer
    </button>
  );
};
