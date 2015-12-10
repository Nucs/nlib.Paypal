# nlib.Paypal
nlib.Paypal aims to provide a simple but yet clever way to handle paypal IPN messages.
see PaypalHandler.cs for the basic abstract structure of this lib.
The PaypalHandler provides 3 basic abstract methoods: `Verified`, `Failed` and `Unhandled`; And 1 abstract property `ToBePayedEmail`.

## Abstract implementation
* `Verified` - is called after the payment is verified to be targeting `ToBePayedEmail` property AND after Paypal itself verified it is not a fake.
* `Failed` - Occurs when the verification with Paypal fails or the payment receiver is not the `ToBePayedEmail`
* `Unhandled` - Any unexpected exception will go through this method.
* `ToBePayedEmail` - Is the paypal email account that is supposed to receive the payment.


---
See the example dir that is taken from my real-world application from our private game server `sapphirerz.at`.

License: MIT license