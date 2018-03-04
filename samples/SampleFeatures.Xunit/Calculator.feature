Feature: xUnit Calculator

Scenario: Add 5 and 10
	Given I have entered 5
	And I have entered 10
	When I click add
	Then the result should be 15

Scenario: Add 5 and 15
	Given I have entered 5
	And I have entered 15
	When I click add
	Then the result should be 20
