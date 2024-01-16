import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import { Redirect, useParams } from "react-router-dom";
import AppContext from "../../Shared/AppContext";
import {
  AddToGroupModel,
  GroupModel,
  IRouteParams,
  RequestModel,
} from "../../types";
import { GroupInfo } from "../Group/GroupInfo";
import { GroupLookup } from "../Group/GroupLookup";
import { CardColumns } from "reactstrap";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { usePromiseNotification } from "../../util/Notifications";
import { authenticatedFetch } from "../../util/api";
import { notEmptyOrFalsey } from "../../util/ValueChecks";
import SshKeyInput from "../../Shared/SshKeyInput";

export const AccountInfo = () => {
  const [notification, setNotification] = usePromiseNotification();
  const [context, setContext] = useContext(AppContext);
  const { cluster } = useParams<IRouteParams>();
  const account = context.accounts.find((a) => a.cluster === cluster);

  const currentGroups = useMemo(() => account?.memberOfGroups ?? [], [account]);

  const [groups, setGroups] = useState<GroupModel[]>([]);
  useEffect(() => {
    const fetchGroups = async () => {
      const response = await authenticatedFetch(`/api/${cluster}/group/groups`);

      if (response.ok) {
        setGroups(
          ((await response.json()) as GroupModel[]).filter(
            (g) =>
              !currentGroups.some((cg) => cg.id === g.id) &&
              !context.openRequests.some((r) => r.groupModel.id === g.id)
          )
        );
      } else {
        alert("Error fetching groups");
      }
    };

    fetchGroups();
  }, [cluster, context.openRequests, currentGroups]);

  const [getGroupConfirmation] = useConfirmationDialog<AddToGroupModel>(
    {
      title: "Request Access to Group",
      message: (setReturn) => {
        return (
          <div className="row justify-content-center">
            <div className="col-md-8">
              <div className="form-group">
                <GroupLookup
                  setSelection={(selection) =>
                    setReturn((model) => ({ ...model, groupId: selection?.id }))
                  }
                  options={groups}
                />
              </div>
              <div className="form-group">
                <label className="form-label">
                  Who is your supervising PI?
                </label>
                <input
                  className="form-control"
                  id="supervisingPI"
                  placeholder="Supervising PI"
                  onChange={(e) =>
                    setReturn((model) => ({
                      ...model,
                      supervisingPI: e.target.value,
                    }))
                  }
                ></input>
                <p className="form-helper">
                  Some clusters may require additional clarification on who your
                  supervising PI will be for this group. If you are unsure,
                  please ask your sponsor.
                </p>
              </div>
            </div>
          </div>
        );
      },
      canConfirm: (returnValue) => returnValue !== undefined,
    },
    [groups]
  );

  const [getSshKeyConfirmation] = useConfirmationDialog<string>(
    {
      title: "Update your SSH Key",
      message: (setReturn) => {
        return (
          <div className="row justify-content-center">
            <div className="col-md-8">
              <div className="form-group">
                <label className="form-label">
                  Please paste your public SSH key.
                </label>
                <SshKeyInput onChange={setReturn} />
                <p className="form-helper">
                  To generate a ssh key pair please see this link:{" "}
                  <a
                    href="https://hpc.ucdavis.edu/faq#ssh-key"
                    target={"blank"}
                  >
                    https://hpc.ucdavis.edu/faq#ssh-key
                  </a>
                </p>
              </div>
            </div>
          </div>
        );
      },
      canConfirm: (returnValue) => notEmptyOrFalsey(returnValue),
    },
    []
  );

  const handleRequestAccess = useCallback(async () => {
    const [confirmed, addToGroupModel] = await getGroupConfirmation();
    if (confirmed) {
      const request = authenticatedFetch(
        `/api/${cluster}/group/requestaccess/`,
        {
          method: "POST",
          body: JSON.stringify(addToGroupModel),
        }
      );

      setNotification(request, "Sending Request", "Request Sent", async (r) => {
        if (r.status === 400) {
          const errorText = await response.text(); //Bad Request Text
          return errorText;
        } else {
          return "An error happened, please try again.";
        }
      });

      const response = await request;
      if (response.ok) {
        const newRequest = (await response.json()) as RequestModel;
        setContext((c) => ({
          ...c,
          openRequests: [...c.openRequests, newRequest],
        }));
        setGroups((g) => g.filter((g) => g.id !== addToGroupModel.groupId));
      }
    }
  }, [cluster, getGroupConfirmation, setNotification, setContext]);

  const handleUpdateSshKey = useCallback(async () => {
    const [confirmed, sshKey] = await getSshKeyConfirmation();

    if (confirmed) {
      const request = authenticatedFetch(`/api/${cluster}/account/updatessh`, {
        method: "POST",
        body: JSON.stringify({
          accountId: account?.id,
          sshKey: sshKey,
        }),
      });

      setNotification(request, "Sending Request", "Request Sent", async (r) => {
        if (r.status === 400) {
          const errorText = await response.text(); //Bad Request Text
          return errorText;
        } else {
          return "An error happened, please try again.";
        }
      });

      const response = await request;
    }
  }, [account?.id, cluster, getSshKeyConfirmation, setNotification]);

  if (!account) {
    const request = context.openRequests.find(
      (r) => r.cluster === cluster && r.action === "CreateAccount"
    );
    if (request) {
      return <Redirect to={`/${cluster}/pendingapproval`} />;
    }
    return <Redirect to={`/${cluster}/create`} />;
  }

  return (
    <>
      <div className="row justify-content-center">
        <div className="col-md-8">
          {currentGroups.length ? (
            <p>
              Welcome {context.user.detail.firstName}. Your account is
              registered with the following group(s):
            </p>
          ) : (
            <p>
              Welcome {context.user.detail.firstName}. Your account is not
              associated with any groups.
            </p>
          )}

          <CardColumns>
            {currentGroups.map((g, i) => (
              <div className="group-card-admin" key={i}>
                <GroupInfo group={g} />
              </div>
            ))}
          </CardColumns>

          {Boolean(context.openRequests.length) && (
            <>
              <p>You have pending requests for the following group(s):</p>

              <CardColumns>
                {context.openRequests.map((r, i) => (
                  <div className="group-card-admin" key={i}>
                    <GroupInfo group={r.groupModel} />
                  </div>
                ))}
              </CardColumns>
            </>
          )}
          <br />

          <div>
            <button
              disabled={notification.pending || groups.length === 0}
              onClick={() => handleRequestAccess()}
              className="btn btn-primary btn-sm"
            >
              Request Access to Another Group
            </button>{" "}
            <button
              disabled={notification.pending || groups.length === 0}
              onClick={() => handleUpdateSshKey()}
              className="btn btn-primary btn-sm"
            >
              Update SSH Key
            </button>
          </div>
        </div>
      </div>
    </>
  );
};
