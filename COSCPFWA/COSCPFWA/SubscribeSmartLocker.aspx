<%@ Page Title="Subscribe to SmartLocker" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SubscribeSmartLocker.aspx.cs" Inherits="COSCPFWA.SubscribeSmartLocker" %>

<asp:Content ID="HeadContent" ContentPlaceholderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-5">
        <div class="text-center mb-4">
            <h2 class="display-4">Subscribe to Smart Locker</h2>
            <p class="lead">Select a locker location and get access to 24/7 package retrieval.</p>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="message-label alert alert-info mb-4" Visible="false"></asp:Label>

        <div class="row justify-content-center">
            <div class="col-md-6">
                <div class="card shadow-lg">
                    <div class="card-header text-white text-center" style="background-color:#0d47a1;">
                        <h5 class="card-title mb-0">SmartLocker Subscription</h5>
                    </div>
                    <div class="card-body">
                        <div class="form-group text-center">
                            <asp:Label ID="LockerLocationLabel" runat="server" AssociatedControlID="ddlLockerLocation" Text="Locker Location"></asp:Label>
                            <div class="d-flex justify-content-center">
                                <asp:DropDownList ID="ddlLockerLocation" runat="server" CssClass="form-control w-50">
                                    <asp:ListItem Text="Select Location" Value="" />
                                </asp:DropDownList>
                            </div>
                        </div>


                        <div class="form-group text-center mt-4">
                            <asp:Button ID="btnSubscribe" runat="server" Text="Subscribe" CssClass="btn btn-primary btn-lg"
                                OnClientClick="return openPaymentModal();" style="background-color:#0d47a1;" />
                            <input type="hidden" id="confirmPaymentField" name="confirmPayment" value="" />
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </div>

    <!-- Payment section -->
    <div class="modal fade" id="paymentModal" tabindex="-1" aria-labelledby="paymentModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="paymentModalLabel">Payment Details</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <h6 class="mb-4">Please provide payment information to confirm your subscription.</h6>

                    <div class="mb-3">
                        <label for="creditCardNumber" class="form-label">Credit Card Number</label>
                        <input type="text" class="form-control" id="creditCardNumber" placeholder="1234 5678 9876 5432" />
                    </div>
                    <div class="mb-3">
                        <label for="expiryDate" class="form-label">Expiry Date</label>
                        <input type="text" class="form-control" id="expiryDate" placeholder="MM/YY" />
                    </div>
                    <div class="mb-3">
                        <label for="cvv" class="form-label">CVV</label>
                        <input type="text" class="form-control" id="cvv" placeholder="123" />
                    </div>
                    <div class="mb-3">
                        <label for="amount" class="form-label">Total Amount</label>
                        <input type="text" class="form-control" id="amount" value="$9.99" disabled />
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <button type="button" class="btn btn-success" onclick="confirmPayment()">Confirm Payment</button>
                </div>
            </div>
        </div>
    </div>

</asp:Content>

<asp:Content ID="Scripts" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js" defer></script>
    <script>
        function openPaymentModal() {
            const lockerLocation = document.getElementById('<%= ddlLockerLocation.ClientID %>').value;
            if (!lockerLocation) {
                alert("Please select a locker location.");
                return false;
            }
            const paymentModal = new bootstrap.Modal(document.getElementById('paymentModal'));
            paymentModal.show();
            return false;
        }

        function confirmPayment() {
            document.getElementById('confirmPaymentField').value = 'true';
            document.forms[0].submit();
        }
    </script>
</asp:Content>
