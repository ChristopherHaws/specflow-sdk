using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace SampleFeatures
{
	[Binding]
	public class CalculatorSteps : TechTalk.SpecFlow.Steps
	{
		private IList<Int32> numbers = new List<Int32>();
		private Int32 actual;

		[Given(@"I have entered (.*)")]
		public void IHaveEntered(Int32 number)
		{
			this.numbers.Add(number);
		}

		[When(@"I click add")]
		public void IClickAdd()
		{
			this.actual = this.numbers.Sum();
		}

		[Then(@"the result should be (.*)")]
		public void TheResultShouldBe(Int32 expected)
		{
			this.actual.Should().Be(expected);
		}
	}
}
