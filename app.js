var debit = 0;
var credit = 0;
var accountBalance = 5000;

// Check Balance
let balanceButton = document.querySelector("#balanceButton");

balanceButton.addEventListener("click", () => {
    document.querySelector("#message").textContent =
        "Your account balance is: " + accountBalance;
});

let transferButton = document.querySelector("#transferButton");

transferButton.addEventListener("click", () => {

    const transferAmount = Number(prompt("Enter the amount:"));

    accountBalance = accountBalance - transferAmount;

    document.querySelector("#message").textContent =
        "Your current balance is: " + accountBalance;
});

let loanButton = document.querySelector("#loanButton");

loanButton.addEventListener("click", () => {

    const transferAmount = Number(prompt("Enter the amount:"));

    accountBalance = accountBalance + transferAmount;

    document.querySelector("#message").textContent =
        "Your current balance is: " + accountBalance;
});